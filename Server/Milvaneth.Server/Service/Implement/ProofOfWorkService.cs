using Milvaneth.Common;
using Milvaneth.Server.Statics;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Milvaneth.Server.Service
{
    public class ProofOfWorkService : IPowService
    {
        private readonly ITimeService _time;
        private readonly ConcurrentDictionary<long, long> _powRecord;
        private readonly ConcurrentQueue<TimedNonce> _powBlacklist;

        public int Difficulty { get; set; }

        public ProofOfWorkService(ITimeService time)
        {
            _time = time;
            _powRecord = new ConcurrentDictionary<long, long>();
            _powBlacklist = new ConcurrentQueue<TimedNonce>();

            Task.Run(() =>
            {
                for (;;)
                {
                    Thread.Sleep(GlobalConfig.POW_LIFE_TIME * 1000);
                    var now = Helper.DateTimeToUnixTimeStamp(_time.UtcNow);

                    while (_powBlacklist.TryDequeue(out var record) && (now - record.Time) > GlobalConfig.POW_LIFE_TIME) { }
                }
            });
        }

        public bool Verify(byte[] proof)
        {
            if (!GlobalConfig.POW_ENABLE)
                return true;

            if (proof == null)
                return Difficulty <= GlobalConfig.POW_REQUIRED_THRESHOLD;

            if (proof.Length != GlobalConfig.POW_LENGTH)
                return false;

            var nonce = BitConverter.ToInt64(proof, 1 + 4);
            var fingerprint = BitConverter.ToInt64(proof, 0);

            if (!_powRecord.TryGetValue(nonce, out var record) || record != fingerprint)
                return false;

            if (_powBlacklist.Any(x => x.Nonce == nonce))
                return false;

            var now = Helper.DateTimeToUnixTimeStamp(_time.UtcNow);
            var time = now - Little2Int(proof, 1);
            if (time < 0 || time > GlobalConfig.POW_LIFE_TIME)
                return false;

            if (proof[0] < Difficulty)
                return false;

            HashAlgorithm hash = new SHA1CryptoServiceProvider();
            var valid = IsValidPow(hash.ComputeHash(proof), proof[0]);

            if (valid)
            {
                _powBlacklist.Enqueue(new TimedNonce{ Nonce = nonce, Time = now});
            }

            return valid;
        }

        public byte[] Generate(byte difficulty)
        {
            var nonce = new byte[8];
            var rng = new Random();
            rng.NextBytes(nonce);

            var header = new byte[GlobalConfig.POW_HEADER_LENGTH];
            header[0] = difficulty;
            Buffer.BlockCopy(Int2Little((int)Helper.DateTimeToUnixTimeStamp(_time.UtcNow)), 0, header, 1, 4);
            Buffer.BlockCopy(nonce, 0, header, 1 + 4, 8);

            _powRecord[BitConverter.ToInt64(header, 1 + 4)] = BitConverter.ToInt64(header, 0);

            return header;
        }

        public bool ConditionalGenerate(out byte[] header)
        {
            header = null;

            if (GlobalConfig.POW_ENABLE && Difficulty > GlobalConfig.POW_REQUIRED_THRESHOLD)
                header = Generate((byte)Difficulty);

            return GlobalConfig.POW_ENABLE && Difficulty > GlobalConfig.POW_REQUIRED_THRESHOLD;
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

        private class TimedNonce
        {
            public long Time;
            public long Nonce;
        }
    }
}
