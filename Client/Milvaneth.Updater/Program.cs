using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;

namespace Milvaneth.Updater
{
    // milself_*.pack = self update (execute Milvaneth.Updater.exe inside with inherited arguments and `--selfupd` flag)
    // milpatch_*.pack = installation update
    // milinit_*.pack & milupd_*.pack = data update (will merge iconstore)
    // note that self update procedure is exploitable, as no check is done against the new Milvaneth.Updater.exe
    class Program
    {
        private static string _targetFolder;
        private static string _dataFolder;
        private static string _patchFolder;
        private static bool _dataPatch;
        private static bool _binaryPatch;
        private static bool _selfPatch;
        private static bool _selfPatchAgent;
        private static bool _restartProgram;

        static void Main(string[] args)
        {
            string method = null;

            var p = new OptionSet()
                .Add("target=", v => _targetFolder = v)
                .Add("data=", v => _dataFolder = v)
                .Add("source=", v => _patchFolder = v)
                .Add("method=", v => method = v)
                .Add("selfupd", v => _selfPatchAgent = true)
                .Add("restart", v => _restartProgram = true);
            p.Parse(args);

            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator))
            {
                Console.WriteLine("Insufficient Privilege");
                Environment.Exit(0);
            }

            if (_targetFolder == null || _dataFolder == null || _patchFolder == null || method == null)
            {
                Console.WriteLine("Unrecognized Arguments");
                Environment.Exit(0);
            }

            if (!Directory.Exists(_targetFolder) || !Directory.Exists(_dataFolder) || !Directory.Exists(_patchFolder))
            {
                Console.WriteLine("Invalid Arguments");
                Environment.Exit(0);
            }

            _dataPatch = method.Contains("d");
            _binaryPatch = method.Contains("b");
            _selfPatch = method.Contains("u");

            var selfid = Process.GetCurrentProcess().Id;
            Thread.Sleep(2000);

            if (Process.GetProcessesByName("Milvaneth").Length > 0 ||
                Process.GetProcessesByName("Milvaneth.Cmd").Length > 0 ||
                Process.GetProcessesByName("Milvaneth.Updater").Any(x => x.Id != selfid)) 
            {
                Thread.Sleep(5000);
                Process.GetProcessesByName("Milvaneth").ToList().ForEach(x => x.Kill());
                Process.GetProcessesByName("Milvaneth.Cmd").ToList().ForEach(x => x.Kill());
                Process.GetProcessesByName("Milvaneth.Updater").Where(x => x.Id != selfid).ToList().ForEach(x => x.Kill());
            }

            if (_selfPatch && _selfPatchAgent)
            {
                var tempFolder = Path.GetDirectoryName(Process.GetCurrentProcess().GetMainModuleFileName());
                try
                {
                    Helper.DirectoryCopy(tempFolder, _targetFolder, true);
                    Directory.Delete(_patchFolder, true);
                }
                catch
                {
                    // ignored
                }

                if (_restartProgram)
                {
                    Process.Start(Path.Combine(_targetFolder, "Milvaneth.exe"));
                }

                var proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        Arguments = $"/C choice /C Y /N /D Y /T 3 & rmdir /S /Q \"{tempFolder}\"",
                        WindowStyle = ProcessWindowStyle.Hidden,
                        CreateNoWindow = true,
                        RedirectStandardOutput = false,
                        RedirectStandardError = false,
                        FileName = "cmd.exe"
                    }
                };
                proc.Start();
                
                return;
            }

            if (_dataPatch)
            {
                var initPack = Directory.EnumerateFiles(_patchFolder, "milinit_*.pack");
                var updPack = Directory.EnumerateFiles(_patchFolder, "milupd_*.pack");
                var packs = initPack.Concat(updPack).OrderBy(x => x);

                if (packs.Any())
                {
                    foreach (var pack in packs)
                    {
                        var dest = Path.Combine(_dataFolder, Path.GetFileName(pack));
                        try
                        {
                            File.Copy(pack, dest, true);
                            Helper.UnzipAndRemove(dest);
                            File.Delete(pack);
                        }
                        catch (InvalidDataException)
                        {
                            File.Delete(pack);
                            File.Delete(dest);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }

                var iconPack = Directory.EnumerateFiles(_dataFolder, "iconstore_delta_*.pack").OrderBy(x => x);

                if (iconPack.Any())
                {
                    var dest = Path.Combine(_dataFolder, "iconstore.pack");
                    foreach (var pack in iconPack)
                    {
                        try
                        {
                            Helper.MergeZip(dest, pack);
                            File.Delete(pack);
                        }
                        catch (InvalidDataException)
                        {
                            File.Delete(pack);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }

            if (_binaryPatch)
            {
                var packs = Directory.EnumerateFiles(_patchFolder, "milpatch_*.pack").OrderBy(x => x);
                if (packs.Any())
                {
                    foreach (var pack in packs)
                    {
                        var dest = Path.Combine(_targetFolder, Path.GetFileName(pack));
                        try
                        {
                            File.Copy(pack, dest, true);
                            Helper.UnzipAndRemove(dest);
                            File.Delete(pack);
                        }
                        catch (InvalidDataException)
                        {
                            File.Delete(pack);
                            File.Delete(dest);
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }

            if (_selfPatch && !_selfPatchAgent)
            {
                var packs = Directory.EnumerateFiles(_patchFolder, "milself_*.pack").OrderByDescending(x => x);
                var patch = packs.FirstOrDefault();
                if (patch != null)
                {
                    var folder = "";
                    do
                    {
                        folder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    } while (Directory.Exists(folder));
                    
                    try
                    {
                        Directory.CreateDirectory(folder);
                        var file = Path.Combine(folder, Path.GetFileName(patch));
                        File.Copy(patch, file, true);
                        Helper.UnzipAndRemove(file);
                        File.Delete(patch);
                        var newPatcher = Path.Combine(folder, "Milvaneth.Updater.exe");
                        if (File.Exists(newPatcher))
                        {
                            var proc = new Process
                            {
                                StartInfo =
                                {
                                    FileName = newPatcher,
                                    UseShellExecute = false,
                                    CreateNoWindow = true,
                                    RedirectStandardOutput = false,
                                    RedirectStandardError = false,

                                    Arguments = string.Join(" ", args) + " --selfupd"
                                }
                            };

                            proc.Start();
                            return;
                        }
                    }
                    catch
                    {
                        Directory.Delete(folder, true);
                    }
                }
            }

            try
            {
                Directory.Delete(_patchFolder, true);
            }
            catch
            {
                // ignored
            }

            if (_restartProgram)
            {
                Process.Start(Path.Combine(_targetFolder, "Milvaneth.exe"));
            }
        }
    }
}
