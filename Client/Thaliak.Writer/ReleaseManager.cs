using System;
using Milvaneth.Common;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace Thaliak.Writer
{
    internal static class ReleaseManager
    {
        static string solutionRoot = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().GetMainModuleFileName()), @"..\..\..\.."));
        public static string BuildBinary()
        {
#if DEBUG
            var mode = "Debug";
#else
            var mode = "Release";
#endif
            var context = Path.Combine(@"bin\x64", mode);

            var release = Path.Combine(solutionRoot, "ReleaseBuild");

            if (Directory.Exists(release))
            {
                Directory.Delete(release, true);
            }

            Thread.Sleep(1000);
            Directory.CreateDirectory(release);
            Thread.Sleep(1000);
            CopyFile("Milvaneth");
            CopyFile("Milvaneth.Cmd");
            CopyFile("Milvaneth.Updater");
            CopyFile("Milvaneth.Overlay");

            return release;

            void CopyFile(string project)
            {
                var dlls = Directory.EnumerateFiles(Path.Combine(solutionRoot, project, context), "*.dll");
                var exes = Directory.EnumerateFiles(Path.Combine(solutionRoot, project, context), "*.exe");
                var configs = Directory.EnumerateFiles(Path.Combine(solutionRoot, project, context), "*.config");
                foreach (var i in dlls.Union(exes).Union(configs))
                {
                    var target = Path.Combine(release, Path.GetFileName(i));
                    if (!File.Exists(target))
                    {
                        File.Copy(i, target);
                    }
                    else
                    {
                        if (Helper.Sha1Hash(File.ReadAllBytes(i)) != Helper.Sha1Hash(File.ReadAllBytes(target)))
                        {
                            throw new IOException("Conflict File Version");
                        }
                    }
                }
            }
        }

        public static void DoRelease(string pack)
        {
            var release = Path.Combine(solutionRoot, "ReleaseBuild");

            if(pack != null)
            {
                File.Copy(pack, Path.Combine(release, Path.GetFileName(pack)), true);
            }

            var delta = $"milpatch_{MilvanethVersion.VersionNumber}.pack";
            using (var deltaStm = new FileStream(delta, FileMode.Create))
            using (var deltaArc = new ZipArchive(deltaStm, ZipArchiveMode.Create))
            {
                foreach (var item in Directory.EnumerateFiles(release))
                {
                    var ent = deltaArc.CreateEntry(Path.GetFileName(item));
                    using (var stm = ent.Open())
                    {
                        var buf = File.ReadAllBytes(item);
                        stm.Write(buf, 0, buf.Length);
                    }
                }
            }
        }
    }
}
