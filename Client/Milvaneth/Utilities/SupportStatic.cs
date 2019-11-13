using Milvaneth.Common;
using Milvaneth.Communication.Vendor;
using Milvaneth.Interactive;
using Milvaneth.Service;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Milvaneth.Utilities
{
    internal static class SupportStatic
    {
        public static string InstallFolder = Path.GetDirectoryName(Process.GetCurrentProcess().GetMainModuleFileName());
        private static Timer _timer;

        public static bool FirstRunPrepare()
        {
            var dataRoot = Helper.GetMilFilePathRaw("");

            var initList = Directory.EnumerateFiles(InstallFolder, "milinit_*.pack");
            initList = initList.OrderByDescending(x => x);
            if (!initList.Any())
                return Directory.Exists(dataRoot);

            Directory.CreateDirectory(dataRoot);
            var srcFile = initList.First();
            var dstFile = Path.Combine(dataRoot, Path.GetFileName(srcFile));
            File.Copy(srcFile, dstFile, true);
            Helper.UnzipAndRemove(dstFile);
            foreach (var i in initList)
            {
                File.Delete(i);
            }

            return true;
        }

        public static bool RunMinimized(string[] args)
        {
            foreach(var arg in args)
            {
                if (arg == "-silent")
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CheckFileIntegrity()
        {
            var hash = ChecksumConfig.PerformHashing(InstallFolder);
            return MilvanethConfig.Store.Checksum?.Validate(hash) ?? true;
        }

        public static void InitializeAll()
        {
            LoggingManagementService.Initialize();
            DictionaryManagementService.Initialize();
            IconManagementService.Initialize();
            TransmittingManagementService.Initialize();
            SubprocessManagementService.Initialize();
        }

        public static void ExitAll()
        {
            TransmittingManagementService.SendSignal(Signal.CommandParentExit, new[] { Process.GetCurrentProcess().Id.ToString() });

            Thread.Sleep(3000);

            try { TransmittingManagementService.Dispose(); } catch { /* ignored */ }
            try { SubprocessManagementService.Dispose(); } catch { /* ignored */ }
            try { IconManagementService.Dispose(); } catch { /* ignored */ }
            try { DictionaryManagementService.Dispose(); } catch { /* ignored */ }
            try { LoggingManagementService.Dispose(); } catch { /* ignored */ }
        }

        public static bool RemoteInitialize(bool autoUpdateEnabled)
        {
            var startTimeSpan = TimeSpan.FromHours(1);
            var periodTimeSpan = TimeSpan.FromHours(1);

            _timer = new Timer(e =>
            {
                try
                {
                    InlineLogic.UpdateLogic(includeOptional: autoUpdateEnabled);
                }
                catch
                {
                    // ignored
                }
                finally
                {
                    // avoid overlapping
                    _timer.Change((int) periodTimeSpan.TotalMilliseconds, Timeout.Infinite);
                }
            }, null, (int) startTimeSpan.TotalMilliseconds, Timeout.Infinite);

            try
            {
                InlineLogic.UpdateLogic(includeOptional: autoUpdateEnabled);
            }
            catch
            {
                // ignored
            }

            return !ApiVendor.ValidateAndRenewToken();
        }
    }
}
