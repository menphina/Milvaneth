using Microsoft.Win32;
using Milvaneth.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

namespace Milvaneth.Service
{
    internal static class SystemManagementService
    {
        public static bool AllowInfoGather;
        private static string _currentProc = Process.GetCurrentProcess().GetMainModuleFileName();

        public static void SetStartup(bool add)
        {
            var rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            if (add)
                rk?.SetValue("Milvaneth Client",
                    $@"""{_currentProc ?? throw new InvalidOperationException("Main Module Failed")}"" -silent");
            else
                rk?.DeleteValue("Milvaneth Client", false);
        }

        public static bool GetStartup()
        {
            var rk = Registry.CurrentUser.OpenSubKey
                ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

            var val = rk?.GetValue("Milvaneth Client");

            return val != null && ((string)val).StartsWith($@"""{_currentProc}""");
        }

        public static bool? GetActPresent()
        {
            if (!AllowInfoGather) return null;

            return Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Advanced Combat Tracker"));
        }

        public static bool? GetIntlPresent()
        {
            if (!AllowInfoGather) return null;

            return Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "My Games", "FINAL FANTASY XIV - A Realm Reborn"));
        }

        public static bool? TryGetActHistDb(out byte[] compressedData)
        {
            compressedData = null;

            if (!AllowInfoGather) return null;
            if ((!GetActPresent()) ?? true) return false;

            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Advanced Combat Tracker", "Advanced Combat Tracker.historydb.xml");

            if (!File.Exists(path)) return false;

            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var compressStream = new MemoryStream())
            using (var compressor = new DeflateStream(compressStream, CompressionMode.Compress))
            {
                fs.CopyTo(compressor);
                compressor.Close();
                compressedData = compressStream.ToArray();
            }

            return true;
        }
    }
}
