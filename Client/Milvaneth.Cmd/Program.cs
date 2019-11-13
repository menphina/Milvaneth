//#define DEBUG_PARAMTER

using Milvaneth.Common;
using Milvaneth.Common.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Thaliak;
using Thaliak.Network;
using Thaliak.Network.Messages;
using Thaliak.Network.Utilities;

namespace Milvaneth.Cmd
{
    class Program
    {
        private static readonly object Lock = new object();
        private static GameStatusManager _gsm;
        private static GameNetworkMonitor _gnm;
        private static GameMemoryMonitor _gmm;
        private static GameDataManager _gdm;
        private static bool _registered;
        private static bool _scanned;
        private static bool _startedNetwork;
        private static bool _startedMemory;
        private static int _currentPid;
        private static int _parentPid;
        private static int _gamePid;
        private static int _dataBusId;
        private static int _venvId;
        private static bool _debugLog;
        private static bool _logLines;

        static void Main(string[] args)
        {
            int counter = 0;
            try
            {
                SetupEnvironment(args);
                RegisterNotification();

                #region Register

                _gsm = new GameStatusManager(_gamePid, _parentPid);

                Log.Verbose("Waiting Game Process");

                _gsm.WaitForProcess();

                Log.Verbose($"Found Game Process {_gsm.GameProcess.Id}");

                _gnm = new GameNetworkMonitor(_gsm.GameProcess);
                _gmm = new GameMemoryMonitor(_gsm.GameProcess);
                _gdm = new GameDataManager();

                _gnm.Subscribe(NetworkCharacterName.GetMessageId(), _gdm.HandleNetworkCharacterName);
                _gnm.Subscribe(NetworkItemInfo.GetMessageId(), _gdm.HandleNetworkItemInfo);
                _gnm.Subscribe(NetworkItemPriceInfo.GetMessageId(), _gdm.HandleNetworkItemPriceInfo);
                _gnm.Subscribe(NetworkItemInfoEnd.GetMessageId(), _gdm.HandleNetworkItemInfoEnd);
                _gnm.Subscribe(NetworkLogout.GetMessageId(), _gdm.HandleNetworkLogout);
                _gnm.Subscribe(NetworkMarketHistory.GetMessageId(), _gdm.HandleNetworkMarketHistory);
                _gnm.Subscribe(NetworkMarketListingCount.GetMessageId(), _gdm.HandleNetworkMarketListingCount);
                _gnm.Subscribe(NetworkMarketListing.GetMessageId(), _gdm.HandleNetworkMarketListing);
                _gnm.Subscribe(NetworkMarketResult.GetMessageId(), _gdm.HandleNetworkMarketResult);
                _gnm.Subscribe(NetworkRetainerHistory.GetMessageId(), _gdm.HandleNetworkRetainerHistory);
                _gnm.Subscribe(NetworkRetainerSummary.GetMessageId(), _gdm.HandleNetworkRetainerSummary);
                _gnm.Subscribe(NetworkRetainerSumEnd.GetMessageId(), _gdm.HandleNetworkRetainerSumEnd);
                _gnm.Subscribe(NetworkLobbyService.GetMessageId(), _gdm.HandleLobbyService);
                _gnm.Subscribe(NetworkLobbyCharacter.GetMessageId(), _gdm.HandleLobbyCharacter);
                _gnm.Subscribe(NetworkClientTrigger.GetMessageId(), _gdm.HandleClientTrigger);
                _gnm.Subscribe(NetworkRequestRetainer.GetMessageId(), _gdm.HandleRequestRetainer);
                _gnm.Subscribe(NetworkInventoryModify.GetMessageId(), _gdm.HandleInventoryModify);
                _gnm.Subscribe(NetworkPlayerSpawn.GetMessageId(), _gdm.HandleNetworkPlayerSpawn);

                _gsm.OnLobbyUpdate += LobbyUpdate;
                _gsm.OnWorldStart += WorldStart;
                _gsm.OnWorldStop += WorldStop;
                _gsm.OnGlobalStart += GlobalStart;
                _gsm.OnGlobalStop += GlobalStop;
                _gmm.OnChatlogUpdated += PackLogData;
                _gdm.OnGameLoggingOut += _gsm.SuspectNetworkStop;
                _gdm.OnDataReady += IpcClient.SendData;
                _gdm.OnRequestScan += _gmm.GetInventory;
                _gsm.GetPacketAmount += () => _gnm.PacketsAnalyzed;

                #endregion

                _gsm.StartMonitoringService();

                Log.Warning("Main thread started");

                while (!_gsm.Stopping)
                {
                    counter++;
                    if (counter > 60)
                    {
                        counter = 0;
                        Log.Debug(
                            $"SysLoad: {_gnm?.PacketsObserved:x}" +
                            $"/{_gnm?.PacketsCaptured}" +
                            $"/{_gnm?.PacketsAnalyzed}" +
                            $"/{_gnm?.MessagesProcessed}" +
                            $"/{_gnm?.MessagesDispatched}" +
                            $"/{_gmm?.ScanCycles}" +
                            $"/{_gmm?.LinesRead}" +
                            $"/{IpcClient.PacketSent}");
                    }

                    _gdm.ScheduledTasks();

                    Thread.Sleep(1000);
                }

                Log.Warning("Main thread expected exiting");
            }
            catch (OperationCanceledException)
            {
                // Well, this IS expected literally.
                Log.Warning("Main thread expected exiting");
            }
            catch (Exception e)
            {
                Log.Error(e, $"Main thread unexpected exiting");
            }
            finally
            {
                try { _gsm.Dispose(); } catch { /* ignored */ }
                try { _gmm.Dispose(); } catch { /* ignored */ }
                try { _gnm.Dispose(); } catch { /* ignored */ }
                try { _gdm.Dispose(); } catch { /* ignored */ }

                IpcClient.SendSignal(Signal.MilvanethSubprocessExit, new[] { "Graceful Exit", "Milvaneth.Cmd", "Main Thread" });
                try { IpcClient.Dispose(); } catch { /* ignored */ }

                Log.Fatal("Main thread exited in Program.Main");
                Environment.Exit(0);
            }
        }

        private static void LobbyUpdate()
        {
            Log.Debug("Lobby Update");
            lock (Lock)
            {
                if (!_gsm.LobbyConnection.Any()) return;

                var filters = FilterBuilder.BuildDefaultFilter(_gsm.LobbyConnection, new List<Connection>());

                if (!_startedNetwork)
                {
                    _gnm.Start(_gsm.LobbyConnection[0].LocalEndPoint.Address, filters);
                    _startedNetwork = true;
                }
                else
                {
                    _gnm.Update(_gsm.LobbyConnection[0].LocalEndPoint.Address, filters);
                }
            }
        }

        private static void WorldStart()
        {
            Log.Debug("World Start");
            lock (Lock)
            {
                var filters = FilterBuilder.BuildDefaultFilter(_gsm.LobbyConnection, _gsm.GameConnection);

                if (!_startedNetwork)
                {
                    _gnm.Start(_gsm.GameConnection[0].LocalEndPoint.Address, filters);
                    _startedNetwork = true;
                }
                else
                {
                    _gnm.Update(_gsm.GameConnection[0].LocalEndPoint.Address, filters);
                }

                if (_startedMemory) return;

                _gdm.Start();
                _gmm.Start(false);

                _startedMemory = true;
            }
        }

        private static void WorldStop()
        {
            Log.Debug("World Stop");
            lock (Lock)
            {
                if (!_startedMemory) return;

                _gmm?.Stop();
                _gdm?.Stop();

                _scanned = false;
                _startedMemory = false;
            }
        }

        private static void GlobalStart()
        {
            Log.Fatal("Logic Start");
            IpcClient.SendSignal(Signal.MilvanethSubprocessReady, new[] { _gsm.GameProcess.Id.ToString() });
        }

        private static void GlobalStop()
        {
            lock (Lock)
            {
                if (_startedNetwork)
                {
                    _gnm.Stop();
                    _startedNetwork = false;
                }

                if (!_startedMemory) return;

                _gmm?.Stop();
                _gdm?.Stop();

                _scanned = false;
                _startedMemory = false;
            }
        }

        private static void Restart(bool purge)
        {
            Log.Debug("World Restart");
            lock (Lock)
            {
                if (!_startedMemory) return;

                _gmm?.Stop(true);
                _gmm?.Start(purge);

                _startedMemory = true;
            }
        }

        private static void RunMemoryTask()
        {
            Log.Debug("Memory Task");
            lock (Lock)
            {
                IpcClient.SendData(new PackedResult(PackedResultType.Status, _gmm.GetStatus()));
                IpcClient.SendData(new PackedResult(PackedResultType.Inventory, _gmm.GetInventory()));
                IpcClient.SendData(new PackedResult(PackedResultType.Artisan, _gmm.GetArtisan()));
            }
        }

        private static void PackLogData(ChatlogResult result)
        {
            if (!_scanned)
            {
                RunMemoryTask();
                _scanned = true;
            }

            IpcClient.SendData(new PackedResult(PackedResultType.Chatlog, result));
        }

        private static void SetupEnvironment(string[] args)
        {
#if DEBUG_PARAMTER
#warning DebugVal
            _parentPid = -1; // -p 1234
            _gamePid = Helper.GetProcess()?.Id ?? -1; // -g 1234
            //_gamePid = -1;
            _dataBusId = -1; // -b 1234
            _venvId = -1; // -v 1234
            _debugLog = false; // --debug
            _logLines = true; // --chatlog

            //_parentPid = 28340;
            //_gamePid = -1;
            //_logLines = false;
            //_debugLog = false;
            //_dataBusId = 2032234920;
            //_venvId = 614562325;
#else
            _parentPid = int.MinValue; // -p 1234, gui process id, -1 = don't watch
            _gamePid = int.MinValue; // -g 1234, game process id, -1 = wait next
            _dataBusId = int.MinValue; // -b 1234, data bus id
            _venvId = int.MinValue; // -v 1234, virtual env id, -1 = don't use
            _debugLog = false; // --debug, log debug information to file
            _logLines = false; // --chatlog, enable chatlog logging (enable debugLog to log to file)
            // don't consider recycle slave instance, just run another

            var p = new OptionSet()
                .Add("p=", v => _parentPid = int.Parse(v))
                .Add("g=", v => _gamePid = int.Parse(v))
                .Add("b=", v => _dataBusId = int.Parse(v))
                .Add("v=", v => _venvId = int.Parse(v))
                .Add("debug", v => _debugLog = true)
                .Add("chatlog", v => _logLines = true);
            p.Parse(args);

            if(_parentPid == int.MinValue || _gamePid == int.MinValue || _dataBusId == int.MinValue || _venvId == int.MinValue)
                Environment.Exit(0);
#endif

            _currentPid = Process.GetCurrentProcess().Id;
            Logger.Initialize(_debugLog, _logLines);

            Log.Fatal(
                $"Process Start: Type Slave /" + 
                $" ProcId {_currentPid} /" + 
                $" ParId {_parentPid} /" +
                $" GameId {_gamePid} /" + 
                $" BusId {_dataBusId} /" + 
                $" Debug {_debugLog} /" +
                $" Chat {_logLines}");

            Helper.SetMilFileVenv(_venvId);
            IpcClient.EnsureBus(_dataBusId);
        }

        private static void RegisterNotification()
        {
            if (_registered) return;

            IpcClient.OnSignalOutput += NotificationListener;
            _registered = true;
        }

        private static void NotificationListener(int sender, Signal sig, DateTime time, string stack, string[] args)
        {
            switch (sig)
            {
                case Signal.CommandParentExit:
                    if (args == null || args.Length < 1 || !int.TryParse(args[0], out var pid1) ||
                        pid1 != _parentPid) return;

                    _gsm.RequireGlobalStop();
                    break;

                case Signal.CommandPurgeCache:
                    if (args == null || args.Length < 1 || !int.TryParse(args[0], out var pid2) ||
                        pid2 != _currentPid) return;

                    Restart(true);
                    break;

                case Signal.CommandReloadCache:
                    Restart(true);
                    break;

                case Signal.CommandRescanMemory:
                    if (args == null || args.Length < 1 || !int.TryParse(args[0], out var pid3) ||
                        pid3 != _currentPid) return;

                    RunMemoryTask();
                    break;

                case Signal.CommandRequireExit:
                    if (args == null || args.Length < 1 || !int.TryParse(args[0], out var pid4) ||
                        pid4 != _currentPid) return;

                    _gsm.RequireGlobalStop();
                    break;

                case Signal.InternalException:
                case Signal.InternalUnmanagedException:
                case Signal.InternalConnectionFin:
                    _gsm.SuspectNetworkStop();
                    break;

                default:
                    break;
            }
        }
    }
}
