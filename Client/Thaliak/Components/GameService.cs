using Milvaneth.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Thaliak.Signatures;

namespace Thaliak.Components
{
    internal sealed class GameService
    {
        private Dictionary<SignatureType, IntPtr> _sigPtr;
        //private Dictionary<ThreadNr, IntPtr> _trdPtr;
        private MemoryService _reader;
        private const string memcachePassword = "";

        public MemoryService Reader => _reader;

        [Obsolete("You are about to initialize an unusable GameService instance.")]
        internal GameService()
        {
        }

        internal GameService(MemoryService ms)
        {
            _sigPtr = new Dictionary<SignatureType, IntPtr>();
            //_trdPtr = new Dictionary<ThreadNr, IntPtr>();
            _reader = ms;

            Signature.LoadSearchPack();
        }
        
        /// <summary>
        /// Initialize the GameScanner instance with a MemoryReader
        /// </summary>
        /// <param name="purgeCache">If true, pointer cache will not be used</param>
        public void Initialize(bool purgeCache)
        {
            Log.Debug($"GS Initialize - Start Initialize");

            //foreach (ThreadNr threadNr in Enum.GetValues(typeof(ThreadNr)))
            //{
            //    if (_trdPtr.ContainsKey(threadNr)) continue;

            //    if (threadNr == ThreadNr.NoThread) continue;

            //    _trdPtr.Add(threadNr, _reader.GetThreadStack((int)threadNr));

            //    Log.Verbose($"GS Initialize - Scanned Thread Stack {(int)threadNr}: {_trdPtr[threadNr].ToInt64():X}");
            //}

            if (!purgeCache && ReadBaseOffsetCache())
                return;

            var unscanned = new List<SigRecord>();

            foreach (SignatureType signatureType in Enum.GetValues(typeof(SignatureType)))
            {
                if (_sigPtr.ContainsKey(signatureType)) continue;

                if (!Signature.SignatureLib.TryGetValue(signatureType, out var pattern)) continue;
                unscanned.Add(pattern);
            }

            var result = Search(ref unscanned);

            Log.Warning($"GS Initialize - Unscanned Signature: {string.Join(", ", unscanned.Select(x => Enum.GetName(typeof(SignatureType), x.SelfType)))}");

            foreach (var kvp in result)
            {
                if(kvp.Value != IntPtr.Zero)
                    _sigPtr.Add(kvp.Key, kvp.Value);
            }

            if (!ValidateSignatures())
            {
                IpcClient.SendSignal(Signal.MilvanethNeedUpdate, new[] { "Memory", "ValidateSignatures" });
                throw new InvalidOperationException("Backbone service self validate failed");
            }

            WriteBaseOffsetCache();

            Log.Debug($"GS Initialize - Finish Initialize");
        }

        /// <summary>
        /// Validate all signatures are found in memory. It not ensures correctness for scan results
        /// </summary>
        /// <returns>True if all signatures are found</returns>
        private bool ValidateSignatures()
        {
            try
            {
                foreach (SignatureType key in Enum.GetValues(typeof(SignatureType)))
                {
                    if (!_sigPtr.ContainsKey(key))
                    {
                        Log.Error($"ValidateSignatures - Missing Signature: {Enum.GetName(typeof(SignatureType), key)}");
                        return false;
                    }

                    if (_sigPtr[key] == IntPtr.Zero)
                    {
                        Log.Error($"ValidateSignatures - Zero Signature: {Enum.GetName(typeof(SignatureType), key)}");
                        return false;
                    }

                    Log.Debug($"ValidateSignatures - Signature Result: {Enum.GetName(typeof(SignatureType), key)} = {_sigPtr[key].ToInt64():X}");
                }

                var servertime = BitConverter.ToInt32(_reader.Read(GetPointer(PointerType.ServerTime), 4), 0);
                Log.Debug($"ValidateSignatures - Server Time: {servertime}");
                var timestamp = Helper.DateTimeToUnixTimeStamp(DateTime.UtcNow);
                Log.Debug($"ValidateSignatures - Local Time: {timestamp}");

                if (Math.Abs(timestamp - servertime) > 60)
                {
                    Log.Error($"ValidateSignatures - Time Mismatch: Expect {timestamp}, Get {servertime}");
                    return false;
                }

                var choco1 = BitConverter.ToInt16(_reader.Read(GetPointer(PointerType.Inventory) + 24 * (int)InventoryContainerOffset.CHOCOBO_BAG_1, 10), 8);

                if (choco1 != (int) InventoryContainerId.CHOCOBO_BAG_1)
                {
                    Log.Error($"ValidateSignatures - Constant Mismatch: Expect {(int)InventoryContainerId.CHOCOBO_BAG_1}, Get {choco1}");
                    return false;
                }
            }
            catch(Exception e)
            {
                Log.Error(e,$"ValidateSignatures - Unhandled Error");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get absolute pointer for specific target
        /// </summary>
        /// <param name="pointerType">Type of the target</param>
        /// <returns>Absolute pointer pointing to the target</returns>
        internal IntPtr GetPointer(PointerType pointerType)
        {
            try
            {
                var key = (SignatureType)((int)pointerType & 0xFF00);

                if (pointerType == PointerType.NotSupported ||
                    !Signature.PointerLib.TryGetValue(pointerType, out var sigRecord) ||
                    !_sigPtr.TryGetValue(key, out var sigPointer))
                    return IntPtr.Zero;

                var noAddBase = false;
                IntPtr entry;
                
                switch (key)
                {
                    case SignatureType.Invalid:
                        entry = IntPtr.Zero;
                        break;
                    case SignatureType.ThreadStack:
                        throw new NotImplementedException("TSPtr not currently supported");
                        noAddBase = true;
                        break;
                    default:
                        entry = sigPointer;
                        break;
                }
                return _reader.TraceTree(entry, sigRecord.Offsets, sigRecord.FinalOffset, noAddBase);
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// Search memory pointers for specific signatures
        /// </summary>
        /// <param name="unscanned">Signatures unscanned</param>
        /// <returns>Relative pointers of signatures</returns>
        private Dictionary<SignatureType, IntPtr> Search(ref List<SigRecord> unscanned)
        {
            const int sliceLength = 65536;
            var moduleMemorySize = _reader.Info.Process.MainModule.ModuleMemorySize;
            var baseAddress = _reader.Info.Process.MainModule.BaseAddress;

            var mainModuleEnd = IntPtr.Add(baseAddress, moduleMemorySize);
            var sliceStart = baseAddress;
            var scanResults = new Dictionary<SignatureType, IntPtr>();

            var maxLength = -1;

            foreach (var sig in unscanned)
            {
                maxLength = maxLength < sig.Length ? sig.Length : maxLength;
            }

            while (sliceStart.ToInt64() < mainModuleEnd.ToInt64() && unscanned.Count > 0)
            {
                var bufferLength = (long) sliceLength;

                var sliceEnd = IntPtr.Add(sliceStart, sliceLength);
                if (sliceEnd.ToInt64() > mainModuleEnd.ToInt64())
                    bufferLength = mainModuleEnd.ToInt64() - sliceStart.ToInt64();

                var sliceBuffer = _reader.Read(sliceStart, bufferLength);

                foreach (var sig in unscanned)
                {
                    var patternLength = sig.Length;
                    var patternOffset = sig.Offset;

                    var index = Searcher.Search(sliceBuffer, sig.Signature, sig.Mask);

                    if (index < 0) continue;

                    long offsetFromBase;
                    if (sig.AsmSignature)
                    {
                        var effectiveAddress = new IntPtr(BitConverter.ToInt32(sliceBuffer,
                            index + patternLength + patternOffset));
                        var realAddress = sliceStart.ToInt64() + index + patternLength + 4L +
                                          effectiveAddress.ToInt64();
                        offsetFromBase = realAddress - baseAddress.ToInt64();
                    }
                    else
                    {
                        var effectiveAddress = new IntPtr(BitConverter.ToInt32(sliceBuffer,
                            index + patternLength + patternOffset));
                        offsetFromBase = effectiveAddress.ToInt64() - baseAddress.ToInt64();
                    }
                    scanResults.Add(sig.SelfType, new IntPtr(offsetFromBase));
                    sig.Finished = true;
                }

                unscanned = unscanned.Where(x => !x.Finished).ToList();
                sliceStart = IntPtr.Add(sliceStart, sliceLength - maxLength);
            }

            return scanResults;
        }

        /// <summary>
        /// Verify game version via ffxivgame.ver
        /// </summary>
        /// <param name="process">Target process</param>
        /// <param name="version">Version string</param>
        /// <returns>Whether version string matches</returns>
        internal bool VersionVerify(string version)
        {
            return GetVersionString().Equals(version);
        }

        internal string GetVersionString()
        {
            var dir = Path.GetDirectoryName(_reader.Info.Process.GetMainModuleFileName());
            var ver = Helper.GetGameVersion(dir);
            Log.Verbose($"GetVersionString - Got {ver}");
            return ver;
        }

        internal bool ReadBaseOffsetCache()
        {
            try
            {
                var pathCache = Helper.GetMilFilePath("memory.pack");

                if (!File.Exists(pathCache))
                {
                    Log.Debug($"ReadBaseOffsetCache - No Cache");
                    return false;
                }

                var cache = new Serializer<ScanCache>(pathCache, memcachePassword);
                var memConf = cache.Load();

                var cacheGameVer = memConf.CacheGameVersion;
                var isOutdated =
                    (DateTime.UtcNow - Helper.UnixTimeStampToDateTime(memConf.LastCacheTime, false)).TotalDays > 30;
                var cacheSigVer = memConf.SignatureVersion;
                var isOutversioned = !VersionVerify(cacheGameVer);

                if (isOutdated || isOutversioned || cacheSigVer != Signature.DataVersion)
                {
                    Log.Debug($"ReadBaseOffsetCache - Outdated (Time {isOutdated} / Ver {isOutversioned} / Sig {cacheSigVer != Signature.DataVersion})");
                    return false;
                }

                // We can cache offsets since they will valid for a version
                _sigPtr = memConf.CacheValue.ToDictionary(x => (SignatureType) x.Key, x => new IntPtr(x.Value));

                if (ValidateSignatures())
                    return true;

                File.Delete(pathCache);

                Log.Debug($"ReadBaseOffsetCache - ValidateSignatures NG");
                return false;
            }
            catch (Exception e)
            {
                Log.Error(e, $"ReadBaseOffsetCache - Unhandled Error");
                return false;
            }
        }

        private void WriteBaseOffsetCache()
        {
            var pathCache = Helper.GetMilFilePath("memory.pack");
            var cache = new Serializer<ScanCache>(pathCache, memcachePassword);
            var memConf = new ScanCache
            {
                CacheValue = _sigPtr.Select(x => new KeyValuePair<int, long>((int) x.Key, x.Value.ToInt64())),
                LastCacheTime = Helper.DateTimeToUnixTimeStamp(DateTime.UtcNow),
                CacheGameVersion = GetVersionString(),
                SignatureVersion = Signature.DataVersion
            };
            cache.Save(memConf);
        }
    }
}