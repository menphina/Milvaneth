using MessagePack;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class ChecksumConfig
    {
        [Key(0)]
        public byte[] SearchPack;
        [Key(1)]
        public byte[] NetworkPack;
        [Key(2)]
        public byte[] DictionaryPack;
        [Key(3)]
        public byte[] Milvaneth;
        [Key(4)]
        public byte[] MilvanethCmd;
        [Key(5)]
        public byte[] MilvanethCommon;
        [Key(6)]
        public byte[] MilvanethCommunication;
        [Key(7)]
        public byte[] ThaliakMemory;
        [Key(8)]
        public byte[] ThaliakNetwork;

        public static ChecksumConfig PerformHashing(string installPath)
        {
            return new ChecksumConfig
            {
                // no apidef.pack since these data are stored in it.
                DictionaryPack = CalculateHash(Helper.GetMilFilePathRaw("dictionary.pack")),
                // no env.pack since it's UGC
                // no iconstore.pack since it's rely on external writer.
                // no memory.pack since it's UGC
                NetworkPack = CalculateHash(Helper.GetMilFilePathRaw("network.pack")),
                SearchPack = CalculateHash(Helper.GetMilFilePathRaw("search.pack")),

                Milvaneth = CalculateHash(Path.Combine(installPath, "Milvaneth.exe")),
                MilvanethCmd = CalculateHash(Path.Combine(installPath, "Milvaneth.Cmd.exe")),
                MilvanethCommon = CalculateHash(Path.Combine(installPath, "Milvaneth.Common.dll")),
                MilvanethCommunication = CalculateHash(Path.Combine(installPath, "Milvaneth.Communication.dll")),
                // no Milvaneth.Overlay.dll since we allow user to replace it. (although not recommended)
                // no Milvaneth.Updater.exe since we may run into trouble while chaining updates.
                ThaliakMemory = CalculateHash(Path.Combine(installPath, "Thaliak.dll")),
                ThaliakNetwork = CalculateHash(Path.Combine(installPath, "Thaliak.Network.dll")),
                // no Thaliak.Private.dll since it will be protected.
                // no Thaliak.Writer.exe since it should not be released.
            };
        }

        public bool Validate(ChecksumConfig checksums)
        {
            try
            {
                return DictionaryPack.SequenceEqual(checksums.DictionaryPack) &&
                   NetworkPack.SequenceEqual(checksums.NetworkPack) &&
                   SearchPack.SequenceEqual(checksums.SearchPack) &&
                   Milvaneth.SequenceEqual(checksums.Milvaneth) &&
                   MilvanethCmd.SequenceEqual(checksums.MilvanethCmd) &&
                   MilvanethCommon.SequenceEqual(checksums.MilvanethCommon) &&
                   MilvanethCommunication.SequenceEqual(checksums.MilvanethCommunication) &&
                   ThaliakMemory.SequenceEqual(checksums.ThaliakMemory) &&
                   ThaliakNetwork.SequenceEqual(checksums.ThaliakNetwork);
            }
            catch
            {
                return false;
            }
        }

        private static byte[] CalculateHash(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 16 * 1024 * 1024))
            {
                var sha = new SHA1Managed();
                return sha.ComputeHash(fs);
            }
        }
    }
}
