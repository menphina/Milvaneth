using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

// This is a partial copy of Milvaneth.Common.Helper

namespace Milvaneth.Updater
{
    public static class Helper
    {
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

        // https://stackoverflow.com/questions/5497064/how-to-get-the-full-path-of-running-process/48319879#48319879
        public static string GetMainModuleFileName(this Process process, int buffer = 1024)
        {
            var fileNameBuilder = new StringBuilder(buffer);
            var bufferLength = (uint)fileNameBuilder.Capacity + 1;
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

        public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            var dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                return;
            }

            var dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            var files = dir.GetFiles();
            foreach (var file in files)
            {
                var temp = Path.Combine(destDirName, file.Name);
                file.CopyTo(temp, false);
            }

            if (!copySubDirs) return;

            foreach (var sub in dirs)
            {
                var temp = Path.Combine(destDirName, sub.Name);
                DirectoryCopy(sub.FullName, temp, copySubDirs);
            }
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

        private static class NativeMethods
        {
            [DllImport("Kernel32.dll")]
            public static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags,
                [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);
        }
    }
}
