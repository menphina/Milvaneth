using Milvaneth.Common;
using Milvaneth.Interactive;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace Milvaneth.Service
{
    internal static class SubprocessManagementService
    {
        public delegate void OnOverrideRequestedDelegate();
        public static OnOverrideRequestedDelegate OnOverrideRequested;

        public static bool UseParent { get; set; }
        public static bool EnableMultiSupport { get; set; }

        private static bool _useHunter;
        public static bool UseHunter
        {
            get => _useHunter;
            set
            {
                _useHunter = value;
                if (_useHunter && _initialized)
                    SpawnHunter();
                if (!_useHunter && _initialized)
                    KillHunter();
            }
        }
        public static bool AutoAttach { get; set; }
        public static bool LogDebugInfo { get; set; }
        public static bool LogChatLogContent { get; set; }

        private static bool _initialized;
        private static string _exePath;
        private static int _parentId;
        private static ProcessResourceEqualityComparer _prec;
        private static MultiSetComparer<int> _prmsc;
        private static ManagementEventWatcher _startWatch;
        private static ManagementEventWatcher _stopWatch;

        private static List<ProcessResource> _processRegistry;

        private const string _exeName = "Milvaneth.Cmd.exe";
        private const string _instanceName = "Milvaneth.Cmd";
        private static readonly string[] _venvRequired = {
            "memory.pack",
        };

        public static void Initialize()
        {
            var proc = Process.GetCurrentProcess();
            var path = proc.GetMainModuleFileName() ??
                       throw new InvalidOperationException("Current process has no main module");

            _exePath = Path.Combine(Path.GetDirectoryName(path), _exeName);
            _parentId = proc.Id;
            _processRegistry = new List<ProcessResource>();
            _prec = new ProcessResourceEqualityComparer();
            _prmsc = new MultiSetComparer<int>();

            _startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            _startWatch.EventArrived += HasProcStart;
            _startWatch.Start();

            _stopWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
            _stopWatch.EventArrived += HasProcStop;
            _stopWatch.Start();

            KillAll();
            Task.Run(() =>
            {
                for (;;)
                {
                    Thread.Sleep(60000);
                    SyncWithSystem();
                }
            });

            if (UseHunter)
            {
                SpawnHunter();
            }

            _initialized = true;

            LoggingManagementService.WriteLine($"{nameof(SubprocessManagementService)} initialized", "SubMgmt");
        }

        public static void Dispose()
        {
            _initialized = false;

            _stopWatch.Stop();
            _startWatch.Stop();
            try { _stopWatch.Dispose(); } catch { /* ignored */ }
            try { _startWatch.Dispose(); } catch { /* ignored */ }

            _stopWatch = null;
            _startWatch = null;
            _prmsc = null;
            _prec = null;
            _processRegistry = null;

            LoggingManagementService.WriteLine($"{nameof(SubprocessManagementService)} uninitialized", "SubMgmt");
        }

        public static bool SpawnAll(out List<int> failed)
        {
            var succ = true;
            var lst = GetFreeGame();
            failed = new List<int>();

            foreach (var p in lst)
            {
                var suc = SpawnSpecific(p);
                succ = suc && succ;
                if (!suc) failed.Add(p);
            }

            return succ;
        }

        public static List<ProcessResource> ListAll()
        {
            return new List<ProcessResource>(_processRegistry);
        }

        public static bool SpawnHunter()
        {
            return HasHunter() || SpawnSpecific(-1);
        }

        public static bool SpawnSpecific(int gameId)
        {
            if (HasSpecific(gameId)) return true;

            try
            {
                var pr = BuildRegistryEntity(gameId, EnableMultiSupport, UseParent, LogDebugInfo, LogChatLogContent);
                return RunNewInstance(pr);
            }
            catch (Exception e)
            {
                LoggingManagementService.WriteLine($"Spawn on Game PID {(gameId > 0 ? gameId.ToString() : "NEXTINSTANCE")} Error: {e.Message}", "SubMgmt");
                return false;
            }
        }

        public static bool HasHunter()
        {
            return HasSpecific(-1);
        }

        public static bool HasSpecific(int gameId)
        {
            return _processRegistry.Any(x => x.gameId == gameId);
        }

        public static bool KillHunter()
        {
            if (HasHunter())
            {
                var pid = _processRegistry.First(x => x.gameId == -1).pid;
                KillSpecific(pid);
            }

            return true;
        }

        public static bool KillSpecific(int pid)
        {
            TransmittingManagementService.SendSignal(Signal.CommandRequireExit, new[] { pid.ToString() });
            return true;
        }

        public static bool ForceKillHunter()
        {
            if (HasHunter())
            {
                var pid = _processRegistry.First(x => x.gameId == -1).pid;
                ForceKillSpecific(pid);
            }

            return true;
        }

        public static bool ForceKillSpecific(int pid)
        {
            _processRegistry.Where(x => x.pid == pid).ToList().ForEach(x =>
            {
                if (!x.proc.HasExited)
                    x.proc.Kill();
            });
            return true;
        }

        public static void UpdateRegistryEntity(int pid, int gameId)
        {
            _processRegistry.Where(x => x.pid == pid).ToList().ForEach(x => x.gameId = gameId);

            var instance = _processRegistry.First(x => x.pid == pid);

            TransmittingManagementService.Register(instance.busSlot, gameId);

            instance.initlized = true;
            OnOverrideRequested?.Invoke();
        }

        private static void KillAll()
        {
            var procList = Process.GetProcessesByName(_instanceName);
            foreach (var i in procList)
            {
                i.Kill();
                i.Dispose();
            }
        }

        private static bool SyncWithSystem()
        {
            if (!_initialized) return false;

            var distinct = _processRegistry.Distinct(_prec).ToList();
            var isUnique = distinct.Count == _processRegistry.Count;
            var procList = Process.GetProcessesByName(_instanceName);
            var procIdList = procList.Select(x => x.Id).ToArray();
            var recList = _processRegistry.Select(x => x.pid);
            var isValid = _prmsc.Equals(procIdList, recList);

            if (isUnique && isValid) return true; // no duplicate, no fault

            if (!isUnique) _processRegistry = distinct; // remove duplicate

            if (isValid) return false; // duplicate but no fault

            var tmp = new List<ProcessResource>();

            foreach (var id in procIdList)
            {
                var dat = _processRegistry.FirstOrDefault(x => x.pid == id); // remove dead proc
                
                if(dat != null)
                    tmp.Add(dat); // add valid proc
                else
                    procList.First(x => x.Id == id).Kill(); // kill zombie proc
            }

            _processRegistry = tmp;

            return false;
        }

        private static ProcessResource BuildRegistryEntity(int gameid, bool useVenv, bool useParent, bool logDebug, bool logChat)
        {
            if (gameid > 0 && _processRegistry.Any(x => x.gameId == gameid)) return null;

            var pr = new ProcessResource();
            var rand = new Random();
            int id;

            do
            {
                id = rand.Next();
            } while (_processRegistry.Any(x => x.busSlot == id));
            pr.busSlot = id;
            TransmittingManagementService.Open(pr.busSlot);

            if (useVenv)
            {
                do
                {
                    id = rand.Next();
                } while (_processRegistry.Any(x => x.venvId == id));
                pr.venvId = id;

                Directory.CreateDirectory(Helper.GetMilFilePathWithVenv("", pr.venvId));

                foreach (var i in _venvRequired)
                {
                    var from = Helper.GetMilFilePathWithVenv(i, -1);
                    var to = Helper.GetMilFilePathWithVenv(i, pr.venvId);

                    if(File.Exists(from))
                        File.Copy(from, to, true);
                }
            }
            else
            {
                pr.venvId = -1;
            }

            pr.gameId = gameid;

            pr.parentId = useParent ? _parentId : -1;

            pr.debugLog = logDebug;

            pr.chatLog = logChat;

            _processRegistry.Add(pr);

            return pr;
        }

        private static void DeleteRegistryEntity(int pid)
        {
            var instance = _processRegistry.FirstOrDefault(x => x.pid == pid);
            if (instance != null && instance.initlized)
            {
                TransmittingManagementService.Close(instance.busSlot);

                var venv = instance.venvId;

                if (venv != -1)
                {
                    foreach (var i in _venvRequired)
                    {
                        var from = Helper.GetMilFilePathWithVenv(i, venv);
                        var to = Helper.GetMilFilePathWithVenv(i, -1);

                        if (File.Exists(from))
                            File.Copy(from, to, true);
                    }

                    Directory.Delete(Helper.GetMilFilePathWithVenv("", venv), true);
                }

                _processRegistry.Where(x => x.pid == pid).ToList().ForEach(x => x.proc.Dispose());
                _processRegistry.RemoveAll(x => x.pid == pid);

                LoggingManagementService.WriteLine("Process Has Exited", pid);
            }
        }

        private static IEnumerable<int> GetFreeGame()
        {
            var pidList = Helper.GetProcessList().Select(x => x.Id);
            var attachedList = _processRegistry.Select(x => x.gameId);
            return pidList.Except(attachedList);
        }

        private static bool RunNewInstance(ProcessResource pr)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    FileName = _exePath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,

                    Arguments =
                        $"-p {pr.parentId} -g {pr.gameId} -b {pr.busSlot} -v {pr.venvId} {(pr.debugLog ? "--debug" : "")} {(pr.chatLog ? "--chatlog" : "")}"
                }
            };

            proc.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    LoggingManagementService.WriteLine(e.Data, pr.pid);
                }
            };
            proc.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    LoggingManagementService.WriteLine(e.Data, pr.pid);
                }
            };
            proc.Exited += (sender, e) =>
            {
                LoggingManagementService.WriteLine("Process Has Exited", pr.pid);
            };

            var stat = proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            pr.pid = proc.Id;
            pr.proc = proc;

            LoggingManagementService.WriteLine("Process Has Started", pr.pid);

            return stat;
        }

        private static void HasProcStart(object sender, EventArrivedEventArgs e)
        {
            if (!AutoAttach && !UseHunter) return;

            var starting = (uint)e.NewEvent.Properties["ProcessID"].Value;
            var procname = (string)e.NewEvent.Properties["ProcessName"].Value;

            if (procname != "ffxiv_dx11.exe" || !Helper.IsChineseVersion(Process.GetProcessById((int)starting))) return;

            Task.Run(() =>
            {
                Thread.Sleep(5000);
                if (AutoAttach && !_processRegistry.Any(x => x.gameId == starting)) SpawnSpecific((int) starting);
                if (UseHunter) SpawnHunter();
            });
        }

        private static void HasProcStop(object sender, EventArrivedEventArgs e)
        {
            var exiting = (uint)e.NewEvent.Properties["ProcessID"].Value;

            DeleteRegistryEntity((int) exiting);
        }

        public sealed class ProcessResource : IEquatable<ProcessResource>
        {
            public int pid;
            public int busSlot;
            public int venvId;
            public int gameId;
            public int parentId;
            public bool debugLog;
            public bool chatLog;

            public bool initlized;
            internal Process proc;

            public bool Equals(ProcessResource obj)
            {
                return obj != null && obj.pid == this.pid;
            }

            public override bool Equals(object obj)
            {
                if (obj == null || GetType() != obj.GetType())
                {
                    return false;
                }

                return obj is ProcessResource pr && Equals(pr);
            }

            public override int GetHashCode()
            {
                var hash = 17;

                unchecked
                {
                    hash = hash * 31 + pid;
                    hash = hash * 31 + busSlot;
                    hash = hash * 31 + venvId;
                    hash = hash * 31 + gameId;
                    hash = hash * 31 + parentId;
                    hash = hash * 31 + (debugLog ? 1 : 0);
                    hash = hash * 31 + (chatLog ? 1 : 0);
                    hash = hash * 31 + proc.GetHashCode();
                }

                return hash;
            }
        }

        public sealed class ProcessResourceEqualityComparer : IEqualityComparer<ProcessResource>
        {
            public bool Equals(ProcessResource b1, ProcessResource b2)
            {
                if (object.ReferenceEquals(b1, b2))
                    return true;

                if (b1 is null || b2 is null)
                    return false;

                return b1.Equals(b2);
            }

            public int GetHashCode(ProcessResource b) => b.GetHashCode();
        }
    }
}
