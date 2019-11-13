using Org.BouncyCastle.Crypto.Agreement.Srp;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using System.Text;

namespace Milvaneth.Communication.Vendor
{
    public class Srp6Vendor
    {
        public static readonly Srp6GroupParameters GroupParameters = Srp6StandardGroups.rfc5054_2048;
        public static readonly int BitLength = GroupParameters.N.BitLength;
        public static BigInteger Srp6Init(string username, byte[] password, out byte[] s)
        {
            var random = new SecureRandom(new CryptoApiRandomGenerator());
            s = new byte[16];
            random.NextBytes(s);

            var I = Encoding.UTF8.GetBytes(username);
            var p = FromInput(password, s);

            var gen = new Srp6VerifierGenerator();
            gen.Init(GroupParameters, new Sha256Digest());

            return gen.GenerateVerifier(s, I, p);
        }

        public static BigInteger Srp6Response(string username, byte[] password, byte[] salt, BigInteger B, out BigInteger token)
        {
            var random = new SecureRandom(new CryptoApiRandomGenerator());

            var I = Encoding.UTF8.GetBytes(username);
            var p = FromInput(password, salt);

            var client = new Srp6Client();
            client.Init(GroupParameters, new Sha256Digest(), random);
            token = client.GenerateClientCredentials(salt, I, p);
            client.CalculateSecret(B);
            return client.CalculateClientEvidenceMessage();
        }

        private static byte[] FromInput(byte[] password, byte[] salt)
        {
            // srp6 verifier is not secure against brute force attack. so we must hash it first
            using (var pv = PasswordVendor.FromInput(password, salt))
            {
                return pv.ToByteArray();
            }
        }
    }
}
