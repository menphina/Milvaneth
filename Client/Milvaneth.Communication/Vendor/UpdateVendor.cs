using System;
using Flurl;
using Milvaneth.Common;
using Milvaneth.Common.Communication.Version;
using Milvaneth.Communication.Download;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Milvaneth.Communication.Vendor
{
    public class UpdateVendor
    {
        public static string TempPath = Helper.GetMilFilePathRaw("updatetemp");
        public static string ProcessRoot = Path.GetDirectoryName(Process.GetCurrentProcess().GetMainModuleFileName());
        public static string DataRoot = Helper.GetMilFilePathRaw("");
        public static string UpdaterPath = Path.Combine(ProcessRoot, "Milvaneth.Update.exe");

        internal static string FormatArgumentString(VersionDownload argument)
        {
            var method = (argument.BinaryUpdate ? "b" : "") + 
                         (argument.DataUpdate ? "d" : "") +
                         (argument.UpdaterUpdate ? "u" : "");
            return argument.Argument
                .Replace("<?installdir>", $"\"{ProcessRoot}\"")
                .Replace("<?datadir>", $"\"{DataRoot}\"")
                .Replace("<?tempdir>", $"\"{TempPath}\"")
                .Replace("<?method>", method);
        }

        public static DownloadInfo[] DownloadPatches(VersionDownload argument, OnFileDownloadCompleteDelegate handle)
        {
            Directory.CreateDirectory(TempPath);

            var dc = new DownloadClient();
            dc.OnFileDownloadComplete = handle;
            var dic = new Dictionary<string, string>(argument.Files.Length);

            foreach (var file in argument.Files)
            {
                var destFile = Path.Combine(TempPath, Path.GetFileName(file));

                int i = 0;
                while (dic.Values.Contains(destFile, StringComparer.InvariantCultureIgnoreCase))
                {
                    var filename = Path.GetFileName(file);
                    destFile = Path.Combine(TempPath, $"{Path.GetFileNameWithoutExtension(filename)}-{i++}{Path.GetExtension(filename)}");
                }

                dic.Add(argument.FileServer.AppendPathSegment(file), destFile);
            }

            return dc.StartDownload(dic);
        }

        public static void WaitAllFinish(DownloadInfo[] infos)
        {
            while (infos.Any(x => !x.Finished))
                Thread.Sleep(1000);
        }

        public static void InvokeUpdateAndExitProgram(VersionDownload argument, bool restartAfterUpdate)
        {
            var proc = new Process
            {
                StartInfo =
                {
                    FileName = UpdaterPath,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,

                    Arguments = argument.Argument + (restartAfterUpdate ? " --restart" : "")
                }
            };

            try
            {
                proc.Start();
                Environment.Exit(0);
            }
            catch
            {
                // ignored
            }
        }
    }
}
