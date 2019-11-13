using Milvaneth.Common;
using SaintCoinach;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;

namespace Thaliak.Writer
{
    internal class Ico
    {
        internal static void BuildPackAndMerge(string gamePath)
        {
            var realm = new ARealmReversed(gamePath, SaintCoinach.Ex.Language.ChineseSimplified);
            var items = realm.GameData.GetSheet<SaintCoinach.Xiv.Item>();

            var save = Helper.GetMilFilePathRaw($"iconstore.pack");
            var old = Helper.GetMilFilePathRaw($"iconstore_old.pack");
            var delta = Helper.GetMilFilePathRaw($"iconstore_delta_{DateTime.Now:yyyyMMdd}.pack");
            using (var saveStm = new FileStream(save, FileMode.Open))
            using (var oldStm = new FileStream(old, FileMode.Open))
            using (var deltaStm = new FileStream(delta, FileMode.Create))
            using (var saveArc = new ZipArchive(saveStm, ZipArchiveMode.Update))
            using (var oldArc = new ZipArchive(oldStm, ZipArchiveMode.Update))
            using (var deltaArc = new ZipArchive(deltaStm, ZipArchiveMode.Create))
            {
                var oldHash = new Dictionary<string, string>(oldArc.Entries.Count);

                foreach (var entry in oldArc.Entries)
                {
                    using (var reader = entry.Open())
                    {
                        oldHash[entry.Name] = Helper.Sha1Hash(Helper.ReadFullStream(reader));
                    }
                }

                foreach (var i in items)
                {
                    var name = i.Name;
                    var key = i.Key;

                    if (string.IsNullOrWhiteSpace(name)) continue;

                    var bytes = ImageToByteArray(i.Icon.GetImage());

                    if (oldHash.TryGetValue($"{key}.png", out var hash) && hash == Helper.Sha1Hash(bytes)) continue;

                    WriteDual(saveArc, deltaArc, key, bytes);
                }
            }

            File.Delete(old);
            File.Copy(save, old);

            byte[] ImageToByteArray(Image imageIn)
            {
                using (var ms = new MemoryStream())
                {
                    imageIn.Save(ms, ImageFormat.Png);
                    return ms.ToArray();
                }
            }
        }

        private static void WriteDual(ZipArchive saveArc, ZipArchive deltaArc, int key, byte[] bytes)
        {
            var ent = deltaArc.CreateEntry($"{key}.png");
            using (var writer = ent.Open())
            {
                writer.Write(bytes, 0, bytes.Length);
            }

            ent = saveArc.GetEntry($"{key}.png") ?? saveArc.CreateEntry($"{key}.png");
            using (var writer = ent.Open())
            {
                writer.SetLength(0);
                writer.Write(bytes, 0, bytes.Length);
            }
        }
    }
}
