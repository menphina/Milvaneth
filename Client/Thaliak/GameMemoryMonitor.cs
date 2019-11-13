using Milvaneth.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Thaliak.Components;
using Thaliak.Converters;
using Thaliak.Readers;

namespace Thaliak
{
    public class GameMemoryMonitor : IDisposable
    {
        public long ScanCycles => _scanCycles;
        public long LinesRead => _linesRead;

        public delegate void OnChatlogUpdatedDelegate(ChatlogResult result);
        public event OnChatlogUpdatedDelegate OnChatlogUpdated;
        
        private readonly Process p;
        private MemoryService ms;
        private DataReader rd;
        private GameService gs;
        private bool _stopping;
        private long _scanCycles;
        private long _linesRead;

        private readonly object _holdLock = new object();
        private bool _holdingData;
        private int _readLenLength;
        private int _privLen;

        public GameMemoryMonitor(Process process)
        {
            if (process == null || process.HasExited || Path.GetFileName(process.GetMainModuleFileName()) != "ffxiv_dx11.exe")
                throw new ArgumentException("Invalid process");

            p = process;
            ms = new MemoryService(p);
        }

        public void Start(bool purgeCache)
        {
            if(p.HasExited) throw new InvalidOperationException("Process has exited");

            _stopping = false;

            gs = new GameService(ms);
            gs.Initialize(purgeCache);
            rd = new DataReader(gs);

            if (_holdingData)
            {
                rd._readLenLength = _readLenLength;
                rd._privLen = _privLen;
                _holdingData = false;
            }
            
            Task.Run(() =>
            {
                Log.Warning("GameMemoryMonitor Output Thread Started");

                while (!_stopping)
                {
                    if (OnChatlogUpdated != null)
                    {
                        lock (_holdLock)
                        {
                            try
                            {
                                var cr = GetChatlog();

                                if (cr.LogLines.Count > 0)
                                {
                                    Interlocked.Add(ref _linesRead, cr.LogLines.Count);
                                    OnChatlogUpdated?.Invoke(cr);
                                }
                            }
                            catch
                            {
                                // ignored
                            }
                            finally
                            {
                                Interlocked.Increment(ref _scanCycles);
                            }
                        }
                    }

                    Thread.Sleep(250);
                }

                Log.Warning("GameMemoryMonitor Output Thread Exited");

                rd = null;
                gs = null;
            });
        }

        public void Stop(bool holdData = false)
        {
            Log.Warning($"GameMemoryMonitor Output Thread Exiting (Hold Data {holdData})");

            if (holdData)
            {
                lock (_holdLock)
                {
                    if (_holdingData)
                        throw new InvalidOperationException("Holding slot currently in use");

                    _readLenLength = rd._readLenLength;
                    _privLen = rd._privLen;
                    _holdingData = true;
                }
            }
            _stopping = true;
        }

        public void Dispose()
        {
            Stop();
            ms?.Dispose();
        }

        public ArtisanResult GetArtisan()
        {
            return ArtisanResultConverter.FromStruct(rd.GetArtisanList());
        }

        // loop run

        public StatusResult GetStatus()
        {
            var ce = rd.GetCharacterExtra();
            var cm = rd.GetCharacterMap();
            var ps = rd.GetPlayerStat();

            var li = StatusLevelInfoConverter.FromStruct(ps);
            var ci = StatusCharaInfoConverter.FromStruct(cm, ce, rd.GetCurrentGil());

            return new StatusResult
            {
                CharacterName = rd.GetPlayerName(),
                CharacterId = ps.CharacterId,
                ServerTime = rd.GetServerTime(),
                SessionTime = rd.GetSessionUpTime(),
                CharacterCurrentWorld = ce.CurrentWorldId,
                CharacterHomeWorld = ce.HomeWorldId,
                LevelInfo = li,
                CharaInfo = ci,
            };
        }

        public InventoryResult GetInventory()
        {
            return rd.GetInventory();
        }

        public InventoryResult GetInventory(InventoryContainerId type)
        {
            return rd.GetInventory(type);
        }

        private ChatlogResult GetChatlog(int timeout = 5000)
        {
            var cts = new CancellationTokenSource(timeout);
            var clr = new ChatlogResult(null, -1);

            var task = Task.Run<ChatlogResult>(() =>
            {
                var buffer = new List<ChatlogLine>();
                var holder = new ChatlogResult(null, -1);

                try
                {
                    holder = rd.GetChatLog();
                    buffer.AddRange(holder.LogLines);

                    while (holder.Remaining > 0)
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        holder = rd.GetChatLog(holder.Remaining);
                        buffer.AddRange(holder.LogLines);
                    }

                    return new ChatlogResult(buffer, holder.Remaining);
                }
                catch
                {
                    return new ChatlogResult(buffer, holder.Remaining);
                }
            }, cts.Token);

            task.Wait(cts.Token);

            return task.Result ?? clr;
        }
    }
}
