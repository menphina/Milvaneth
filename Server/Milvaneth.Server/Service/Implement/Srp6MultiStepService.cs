using Milvaneth.Server.Statics;
using Org.BouncyCastle.Crypto.Agreement.Srp;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Milvaneth.Server.Service
{
    public class Srp6MultiStepService : ISrp6Service
    {
        private readonly SecureRandom _cryptoRandom;
        private readonly ConcurrentDictionary<long, Srp6Context> _sessions;
        private readonly ITimeService _time;
        private readonly Sha256Digest _hash;
        private readonly IPowService _pow;

        public Srp6MultiStepService(ITimeService time, IPowService pow)
        {
            _time = time;
            _pow = pow;
            _cryptoRandom = new SecureRandom(new CryptoApiRandomGenerator());
            _sessions = new ConcurrentDictionary<long, Srp6Context>();
            _hash = new Sha256Digest();

            Task.Run(() =>
            {
                for (;;)
                {
                    Thread.Sleep(GlobalConfig.SRP6_SESSION_LIFE_TIME * 1000);
                    var now = _time.UtcNow;

                    var list = _sessions.Where(x => x.Value.Expire < now).Select(x => x.Key).ToImmutableList();
                    foreach (var key in list)
                    {
                        _sessions.TryRemove(key, out _);
                    }
                }
            });
        }

        public long DoServerResponse(long accountId, int mode, byte[] verifier, out byte[] serverToken)
        {
            long sessionId;

            do
            {
                var sessionIdBytes = new byte[8];
                _cryptoRandom.NextBytes(sessionIdBytes);
                sessionId = BitConverter.ToInt64(sessionIdBytes, 0);
            } while (sessionId == 0 || _sessions.ContainsKey(sessionId));

            var context = new Srp6Context();

            context.AccountId = accountId;
            context.Server = new Srp6Server();
            context.Server.Init(GetStandardGroups(mode), new BigInteger(verifier), _hash, _cryptoRandom);
            serverToken = context.Server.GenerateServerCredentials().ToByteArray();

            context.Expire = _time.UtcNow.AddSeconds(GlobalConfig.SRP6_SESSION_LIFE_TIME);

            _sessions[sessionId] = context;

            CalculateRate(_sessions.Count);

            return sessionId;
        }

        public bool DoServerValidate(long sessionId, byte[] clientToken, byte[] clientEvidence, out long accountId)
        {
            accountId = 0;

            if (!_sessions.TryGetValue(sessionId, out var context) || context == null)
                return false;

            accountId = context.AccountId;

            if (context.Expire < _time.UtcNow)
                return false;

            context.Server.CalculateSecret(new BigInteger(clientToken));
            var success = context.Server.VerifyClientEvidenceMessage(new BigInteger(clientEvidence));

            if (success)
            {
                _sessions.TryRemove(sessionId, out _);
            }

            return success;
        }

        private Srp6GroupParameters GetStandardGroups(int length)
        {
            switch (length)
            {
                case 2048:
                    return Srp6StandardGroups.rfc5054_2048;
                case 3072:
                    return Srp6StandardGroups.rfc5054_3072;
                case 4096:
                    return Srp6StandardGroups.rfc5054_4096;
                case 6144:
                    return Srp6StandardGroups.rfc5054_6144;
                case 8192:
                    return Srp6StandardGroups.rfc5054_8192;
                default:
                    throw new InvalidOperationException("SRP6a Not Valid Standard Group");
            }
        }

        private void CalculateRate(long count)
        {
            if (count > 10000)
            {
                _pow.Difficulty = 30;
                return;
            }

            if (count > 9000)
            {
                _pow.Difficulty = 29;
                return;
            }

            if (count > 8000)
            {
                _pow.Difficulty = 28;
                return;
            }

            if (count > 7000)
            {
                _pow.Difficulty = 27;
                return;
            }

            if (count > 6000)
            {
                _pow.Difficulty = 26;
                return;
            }

            if (count > 5000)
            {
                _pow.Difficulty = 25;
                return;
            }

            if (count > 4000)
            {
                _pow.Difficulty = 24;
                return;
            }

            if (count > 2500)
            {
                _pow.Difficulty = 23;
                return;
            }

            if (count > 1200)
            {
                _pow.Difficulty = 22;
                return;
            }

            if (count > 740)
            {
                _pow.Difficulty = 21;
                return;
            }

            if (count > 280)
            {
                _pow.Difficulty = 20;
                return;
            }

            if (count > 120)
            {
                _pow.Difficulty = 19;
                return;
            }

            _pow.Difficulty = 0;
        }

        private class Srp6Context
        {
            public long AccountId;
            public DateTime Expire;
            public Srp6Server Server;
        }
    }
}
