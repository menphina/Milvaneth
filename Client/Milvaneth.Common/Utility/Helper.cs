using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Milvaneth.Common
{
    public static class Helper
    {
        private static string _venv = "";
        private const long TicksEpochTime = 621355968000000000L;
        private const long TicksPerSecond = 10000000L;
        private static readonly long TicksShift = (DateTime.Now - DateTime.UtcNow).Ticks;

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp, bool local = true)
        {
            var time = new DateTime(TicksEpochTime + TicksPerSecond * unixTimeStamp, DateTimeKind.Utc);

            if (local)
                return time.ToLocalTime();

            return time;
        }

        public static long DateTimeToUnixTimeStamp(DateTime dateTime)
        {
            return (dateTime.ToUniversalTime().Ticks - TicksEpochTime) / TicksPerSecond;
        }

        public static IList<Process> GetProcessList(bool includeDx9 = false)
        {
            var dx9 = includeDx9
                ? Process.GetProcessesByName("ffxiv").Where(x =>
                    !x.HasExited && Path.GetFileName(x.GetMainModuleFileName()) == "ffxiv.exe").ToList()
                : new List<Process>();
            return Process.GetProcessesByName("ffxiv_dx11")
                .Where(x => !x.HasExited && Path.GetFileName(x.GetMainModuleFileName()) == "ffxiv_dx11.exe")
                .Union(dx9).ToList();
        }

        public static Process GetProcess(int pid = 0, bool includeDx9 = false)
        {
            var ffxivProcessList = GetProcessList(includeDx9);
            return pid != 0
                ? ffxivProcessList.FirstOrDefault(x => x.Id == pid)
                : ffxivProcessList.OrderBy(x => x.StartTime).FirstOrDefault(); // Attach to the 'longest lived' session
        }

        public static bool IsChineseVersion(Process p)
        {
            var dir = Path.GetDirectoryName(p.GetMainModuleFileName());
            var file = Path.Combine(dir, @"My Games\FINAL FANTASY XIV - A Realm Reborn\FFXIV.cfg");
            return File.Exists(file) && GetTrace(dir).Any();
        }

        public static long[] GetTrace()
        {
            return new[] {0x12345678L, 0x22345678L};
            //try
            //{
            //    string path;
            //    return ((path = GetGamePath()) != null) ? GetTrace(path).ToArray() : new long[0];
            //}
            //catch
            //{
            //    return new long[0];
            //}
        }

        private static string GetGamePath()
        {
            return null;
        }

        private static IEnumerable<long> GetTrace(string dir)
        {
            try
            {
                var file = Path.Combine(dir, @"My Games\FINAL FANTASY XIV - A Realm Reborn");
                var prefix = "FFXIV_CHR";
                return Directory.EnumerateDirectories(file, prefix + "*").Select(x =>
                    long.TryParse(Path.GetFileName(x)?.Remove(0, prefix.Length) ?? "-1", NumberStyles.HexNumber,
                        new NumberFormatInfo(), out var id)
                        ? id
                        : -1);
            }
            catch
            {
                return new long[0];
            }
        }

        public static void SetMilFileVenv(int id = -1)
        {
            _venv = id != -1 ? $"venv-{id}" : "";
        }

        public static string GetMilFilePath(string filename)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Milvaneth",
                _venv, filename);
        }

        public static string GetMilFilePathRaw(string filename)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Milvaneth",
                filename);
        }

        public static string GetMilFilePathWithVenv(string filename, int id)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Milvaneth",
                id != -1 ? $"venv-{id}" : "", filename);
        }

        public static unsafe string ToUtf8String(byte[] arr, int off, int idx, int len)
        {
            fixed (byte* p = &arr[off])
            {
                return new string((sbyte*) p, idx, len, Encoding.UTF8).Split('\0')[0];
            }
        }

        public static void MergeZip(string dest, string src)
        {
            using (var destStm = new FileStream(dest, FileMode.Open))
            using (var srcStm = new FileStream(src, FileMode.Open))
            using (var destArc = new ZipArchive(destStm, ZipArchiveMode.Update))
            using (var srcArc = new ZipArchive(srcStm, ZipArchiveMode.Read))
            {
                foreach (var entSrc in srcArc.Entries)
                {
                    var key = entSrc.FullName;
                    var entDest = destArc.GetEntry(key) ?? destArc.CreateEntry(key);

                    using (var writer = entDest.Open())
                    using (var reader = entSrc.Open())
                    {
                        var bytes = ReadFullStream(reader);
                        writer.SetLength(0);
                        writer.Write(bytes, 0, bytes.Length);
                    }
                }
            }
        }

        public static byte[] ReadFullStream(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }

        public static string Sha1Hash(byte[] input)
        {
            using (var sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(input);
                var sb = new StringBuilder(hash.Length * 2);

                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        // https://stackoverflow.com/questions/5497064/how-to-get-the-full-path-of-running-process/48319879#48319879
        public static string GetMainModuleFileName(this Process process, int buffer = 1024)
        {
            var fileNameBuilder = new StringBuilder(buffer);
            var bufferLength = (uint) fileNameBuilder.Capacity + 1;
            return NativeMethods.QueryFullProcessImageName(process.Handle, 0, fileNameBuilder, ref bufferLength)
                ? fileNameBuilder.ToString()
                : null;
        }

        public static void UnzipAndRemove(string filePath)
        {
            using (var zipStream = new FileStream(filePath, FileMode.Open))
            using (var archive = new ZipArchive(zipStream))
            {
                archive.ExtractToDirectory(Path.GetDirectoryName(filePath), true);
            }
            File.Delete(filePath);
        }

        // https://stackoverflow.com/questions/14795197/forcefully-replacing-existing-files-during-extracting-file-using-system-io-compr/30425148
        private static void ExtractToDirectory(this ZipArchive archive, string destinationDirectoryName, bool overwrite)
        {
            if (!overwrite)
            {
                archive.ExtractToDirectory(destinationDirectoryName);
                return;
            }

            var di = Directory.CreateDirectory(destinationDirectoryName);
            var destinationDirectoryFullPath = di.FullName;

            foreach (var file in archive.Entries)
            {
                var completeFileName = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, file.FullName));

                if (!completeFileName.StartsWith(destinationDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new IOException("Trying to extract file outside of destination directory.");
                }

                if (file.Name == "")
                {// Assuming Empty for Directory
                    Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
                    continue;
                }
                file.ExtractToFile(completeFileName, true);
            }
        }

        public static string GetGameVersion(string gamePath)
        {
            // behaviour is exactly the same with game implement, including logic and format
            var pattern = new byte[]
            {
                0x2F, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x66, 0x66, 0x31, 0x34, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x2A, 0x72,
                0x65, 0x76
            };

            try
            {
                var file = Path.Combine(gamePath, @"ffxiv_dx11.exe");
                using (var stream = File.Open(file, FileMode.Open, FileAccess.Read))
                {
                    var buffer = new byte[65536 + 60];
                    stream.Read(buffer, 0, buffer.Length);
                    var position = 0;
                    do
                    {
                        for (var i = 0; i < 65536;)
                        {
                            for (var j = 0; j < pattern.Length; j++)
                            {
                                if (buffer[i + j] == pattern[j])
                                {
                                    if (j == pattern.Length - 1)
                                        position = i + j + 1;
                                    continue;
                                }

                                i += j + 1;
                                break;
                            }

                            if (position > 0)
                                break;
                        }

                        if (position > 0)
                            break;

                        Buffer.BlockCopy(buffer, 65536, buffer, 0, 60);
                    } while (stream.Read(buffer, 60, 65536) > 0);

                    if (position <= 0)
                        return null;

                    var str = Encoding.ASCII.GetString(buffer, position, 60).TrimEnd('*').Split('_');

                    var verFile = Path.Combine(gamePath, @"ffxivgame.ver");

                    if (str.Length < 1 || !int.TryParse(str[0], out _))
                        return null;

                    var rev = str[0];
                    var ex0 = "";

                    if (str.Length >= 2 && DateTime.TryParse(str[1], out var date))
                        ex0 = $"{date:yyyy.MM.dd}.0000.0000";

                    if (File.Exists(verFile))
                        ex0 = File.ReadAllText(verFile).Trim();

                    if (string.IsNullOrEmpty(ex0))
                        return null;

                    var patchNum = 1;
                    var patchList = new List<string>();

                    do
                    {
                        var sqpack = Path.Combine(gamePath, @"sqpack", $"ex{patchNum}");
                        var patchVer = Path.Combine(sqpack, $"ex{patchNum}.ver");

                        if (!File.Exists(patchVer))
                        {
                            break;
                        }

                        patchList.Add($" , ex{patchNum}:{File.ReadAllText(patchVer)}");
                        patchNum++;
                    } while (true);

                    return $"{ex0}({rev}{string.Concat(patchList)})";
                }
            }
            catch
            {
                return null;
            }
        }

        private static bool GetGameIsLocaleChina(string gamePath)
        {
            var pattern = new byte[]
            {
                0x63, 0x68, 0x69, 0x6E, 0x61
            };

            if (File.Exists(Path.Combine(Path.GetDirectoryName(gamePath), @"sdo\sdologin\sdologin.exe")))
                return true;

            try
            {
                var file = Path.Combine(gamePath, @"ffxiv_dx11.exe");
                using (var stream = File.Open(file, FileMode.Open, FileAccess.Read))
                {
                    var buffer = new byte[65536 + 5];
                    stream.Read(buffer, 0, buffer.Length);
                    var position = 0;
                    do
                    {
                        for (var i = 0; i < 65536;)
                        {
                            for (var j = 0; j < pattern.Length; j++)
                            {
                                if (buffer[i + j] == pattern[j])
                                {
                                    if (j == pattern.Length - 1)
                                        position++;
                                    else
                                        continue;
                                }

                                i += j + 1;
                                break;
                            }
                        }

                        Buffer.BlockCopy(buffer, 65536, buffer, 0, 5);
                    } while (stream.Read(buffer, 5, 65536) > 0);

                    return position >= 3;
                }
            }
            catch
            {
                return false;
            }
        }

        private static class NativeMethods
        {
            [DllImport("Kernel32.dll")]
            public static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags,
                [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);
        }
    }
}
