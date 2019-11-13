using Milvaneth.Common;
using System;
using System.IO;
using System.IO.Compression;
using File = System.IO.File;

namespace Thaliak.Writer
{
    class Program
    {
        static void Main(string[] args)
        {
            // This is the "dot pack" data generator for Milvaneth program.
            // It directly referenced related code to ensure format consistency.
            // Change data structure with EXTREME caution.

            // %appdata%\Milvaneth, network.pack, search.pack, dictionary.pack, apidef.pakc
            // download from https://bbs.nga.cn/read.php?tid=17470351

            var path = @"FFXIV_ACT_Plugin.dll";
            var buildPath = ReleaseManager.BuildBinary();
            var gamePath = @"FFXIV";
            var lang = "cn".ToLower();
            var fullstore = true; // pack full iconstore

            var conf = new ConfigStore
            {
                Api = new ApiConfig
                {
                    Endpoints = new[]
                    {
                        "https://localhost:44302/", // localtest
                    },
                    Mime = "application/x-msgpack",
                    Prefix = "api", // Endpoint/Prefix, e.g. https://localhost:44302/api
                },
                Global = new GlobalConfig
                {
                    CustomMessage = "", // Displaying message will block interface thread initializing flow to ensure every user received it, use with caution
                    DataVersion = 20190909,
                    GameVersion = 5000, // 4.5.5 = 4550, 5.0 = 5000, used by lobby encryption
                    ProjectUrl = "https://github.com/menphina/"
                },
            };

            var dataVersion = conf.Global.GameVersion;
            Sig.SaveSearchPack(dataVersion);
            Net.SetMessageId(dataVersion);
            Dic.SaveDictionaries(dataVersion, path, gamePath, lang);
            Ico.BuildPackAndMerge(gamePath);
            Cnf.SetConfigStore(conf, buildPath);
            var dataPack = RunPack(fullstore);
            ReleaseManager.DoRelease(dataPack);
        }

        static string RunPack(bool fullstore)
        {
            var packList = new[]
            {
                "search.pack", // mem sig
                "network.pack", // net opcode
                $"dictionary.pack", // ui strings
                $"iconstore{(fullstore ? "" : $"_delta_{DateTime.Now:yyyyMMdd}")}.pack", // icon pack
                "apidef.pack" // api status
            };

            var delta = $"mil{(fullstore ? "init" : "upd")}_{DateTime.Now:yyyyMMdd}.pack";
            using (var deltaStm = new FileStream(delta, FileMode.Create))
            using (var deltaArc = new ZipArchive(deltaStm, ZipArchiveMode.Create))
            {
                foreach (var item in packList)
                {
                    var ent = deltaArc.CreateEntry(item);
                    using (var stm = ent.Open())
                    {
                        var buf = File.ReadAllBytes(Helper.GetMilFilePathRaw(item));
                        stm.Write(buf, 0, buf.Length);
                    }
                }
            }

            return fullstore ? delta : null;
        }
    }
}
