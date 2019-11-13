using System;
using System.Security.Cryptography;
using Milvaneth.Common;

namespace Milvaneth.Communication.Vendor
{
    public class ProofOfWorkVendor
    {
        // difficulty 20 or 21 has no significant effect on UX
        public static byte[] CalculateProofOfWork(byte[] prefix)
        {
            var offset = prefix.Length;
            var data = new byte[prefix.Length + 32];
            var random = new byte[32];
            Buffer.BlockCopy(prefix, 0, data, 0, prefix.Length);

            HashAlgorithm hash = new SHA1CryptoServiceProvider();
            var rng = new Random();

            do
            {
                rng.NextBytes(random);
                Buffer.BlockCopy(random, 0, data, offset, 32);
            } while (!IsValidPow(hash.ComputeHash(data), prefix[0]));

            return data;
        }

        [Obsolete("This version is for test use")]
        public static bool VerifyProofOfWork(byte[] proof)
        {
            var time = (DateTime.Now - Helper.UnixTimeStampToDateTime(Little2Int(proof, 1))).TotalMinutes;
            if (time < 0 || time > 10)
                return false;
            HashAlgorithm hash = new SHA1CryptoServiceProvider();
            return IsValidPow(hash.ComputeHash(proof), proof[0]);
        }

        [Obsolete("This version is for test use")]
        public static byte[] GenerateHeader(int difficulty)
        {
            var tmp = new byte[16];
            var rng = new Random();
            rng.NextBytes(tmp);
            tmp[0] = (byte)difficulty;
            Buffer.BlockCopy(Int2Little((int) Helper.DateTimeToUnixTimeStamp(DateTime.Now)), 0, tmp, 1, 4);
            return tmp;
        }

        private static bool IsValidPow(byte[] data, int difficulty)
        {
            var diffBits = difficulty % 8;
            var diffBytes = (difficulty - diffBits) / 8;

            if (diffBytes > data.Length - 1) return false;

            for (var i = 0; i < diffBytes; i++)
            {
                if (data[i] != 0) return false;
            }

            switch (diffBits)
            {
                case 0:
                    return true;
                case 1:
                    return data[diffBytes] < 128;
                case 2:
                    return data[diffBytes] < 64;
                case 3:
                    return data[diffBytes] < 32;
                case 4:
                    return data[diffBytes] < 16;
                case 5:
                    return data[diffBytes] < 8;
                case 6:
                    return data[diffBytes] < 4;
                case 7:
                    return data[diffBytes] < 2;
            }

            return false;
        }
        private static byte[] Int2Little(int data)
        {
            byte[] b = new byte[4];
            b[0] = (byte)data;
            b[1] = (byte)(((uint)data >> 8) & 0xFF);
            b[2] = (byte)(((uint)data >> 16) & 0xFF);
            b[3] = (byte)(((uint)data >> 24) & 0xFF);
            return b;
        }

        private static int Little2Int(byte[] data, int off)
        {
            return data[off] + (data[off + 1] << 8) + (data[off + 2] << 16) + (data[off + 3] << 24);
        }
    }
}
