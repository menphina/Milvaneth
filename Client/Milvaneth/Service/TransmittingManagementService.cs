using Milvaneth.Common;
using Milvaneth.Interactive;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Milvaneth.Service
{
    internal static class TransmittingManagementService
    {
        public delegate void DataOutput(int gameId, PackedResult data);
        public static DataOutput OnDataOutput;

        private static Dictionary<int, int> _busRegistry;
        private static IpcServer _server;

        public static void Initialize()
        {
            _busRegistry = new Dictionary<int, int>();
            _server = new IpcServer();
            _server.OnDataOutput += (x, y) => OnDataOutput?.Invoke(_busRegistry[x], y);
            _server.OnSignalOutput += NotificationListener;
            LoggingManagementService.WriteLine($"{nameof(TransmittingManagementService)} initialized", "TsmMgmt");
        }

        public static void Dispose()
        {
            try { _server.Dispose(); } catch { /* ignored */ }

            _server = null;
            _busRegistry = null;
            LoggingManagementService.WriteLine($"{nameof(TransmittingManagementService)} uninitialized", "TsmMgmt");
        }

        internal static void SendSignal(Signal sig, string[] args = null, bool noLog = false)
        {
            _server.SendSignal(sig, args, noLog);
        }

        public static void Open(int busId)
        {
            _server.AddBus(busId);
        }

        public static void Register(int busId, int gameId)
        {
            _busRegistry.Add(busId, gameId);
        }

        public static void Close(int busId)
        {
            _server.RemoveBus(busId);
            _busRegistry.Remove(busId);
        }

        private static void NotificationListener(int sender, Signal sig, DateTime time, string stack, string[] args)
        {
            switch (sig)
            {
                case Signal.ClientInsuffcientPrivilege:
                    if (args == null || args.Length < 1 || !int.TryParse(args[0], out _)) return;

                    LoggingManagementService.WriteLine($"Privilege fail on {sender}, user action is needed", "SigMgmt");
                    break;

                case Signal.ClientNetworkFail:
                    if (args == null || args.Length < 1 || !int.TryParse(args[0], out _)) return; // we need validation

                    LoggingManagementService.WriteLine($"Network fail on {sender}, user action is needed", "SigMgmt");
                    break;

                case Signal.ClientProcessDown:
                    if (args == null || args.Length < 1 || !int.TryParse(args[0], out var pid3)) return;

                    LoggingManagementService.WriteLine($"Game process {pid3} has exited", "SigMgmt");
                    break;

                case Signal.ClientPacketParseFail:
                    if (args == null || args.Length < 1) return;

                    LoggingManagementService.WriteLine($"Malformed packet ({args[0]}) on {sender}, data may loss", "SigMgmt");
                    break;

                case Signal.MilvanethSubprocessExit:
                    if (args == null || args.Length < 2) return;

                    LoggingManagementService.WriteLine($"Monitor process {sender}  has exited: {string.Join("|", args)}",
                        "SigMgmt");
                    break;

                case Signal.MilvanethComponentExit:
                    if (args == null || args.Length < 2) return;

                    SubprocessManagementService.KillSpecific(sender);
                    LoggingManagementService.WriteLine($"Monitor component {sender}  has exited: {string.Join("|", args)}",
                        "SigMgmt");
                    break;

                case Signal.MilvanethNeedUpdate:
                    if (args == null || args.Length < 2) return;

                    SubprocessManagementService.KillSpecific(sender);
                    LoggingManagementService.WriteLine($"Update is required by {sender}: {string.Join("|", args)}",
                        "SigMgmt");
                    break;

                case Signal.MilvanethSubprocessReady:
                    if (args == null || args.Length < 1 || !int.TryParse(args[0], out var pid4)) return;

                    SubprocessManagementService.UpdateRegistryEntity(sender, pid4);
                    LoggingManagementService.WriteLine($"Monitor service {sender} has started on {pid4}", "SigMgmt");
                    break;

                default:
                    break;
            }

            Task.Run(() => InlineLogic.PostSignalNotifyLogic(sig));
        }
    }
}
