using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Milvaneth.Communication.Vendor
{
    public class PasswordVendor : IDisposable
    {
        private const string Prefix = "MILPV"; // MILvaneth Password Vendor
        private const string Method = "RFC2898";

        // Default value
        private const int Factor = 14; // 2^14=16384
        private const int HashLength = 32;
        private const int SaltLength = 16;
        private static readonly byte[] Entropy = { 8, 92, 122, 167, 74, 134, 194, 27, 248, 255, 191, 190, 46, 35, 32, 230 };

        private byte[] _pass;
        private int _factor;

        private PasswordVendor(byte[] password)
        {
            _pass = password;
            _factor = password[password[0] + 1];

            if (_factor < 8)
                throw new InvalidOperationException("PasswordVendor - At least 2 ^ 8 iterations is required");
            if (_pass[0] < 8)
                throw new InvalidOperationException("PasswordVendor - At least 8 bytes of salt is required");
            if (_pass.Length - 2 - _pass[0] < 20)
                throw new InvalidOperationException("PasswordVendor - At least 20 bytes of hash is required");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            Clear(_pass, 0, _pass.Length);
            _pass = null;
        }

        public bool Validate(byte[] password)
        {
            var salt = new byte[_pass[0]];
            Buffer.BlockCopy(_pass, 1, salt, 0, salt.Length);

            byte[] hash;
            using (var rfc = new Rfc2898DeriveBytes(password, salt, (int)Math.Pow(2, _factor)))
            {
                hash = rfc.GetBytes(_pass.Length - 2 - salt.Length);
            }

            for (var i = 0; i < hash.Length; i++)
            {
                if (_pass[_pass[0] + 2 + i] != hash[i])
                    return false;
            }

            Clear(salt, 0, salt.Length);
            Clear(hash, 0, hash.Length);

            return true;
        }

        public static PasswordVendor FromInput(byte[] password, int factor = Factor, int saltLength = SaltLength, int hashLength = HashLength)
        {
            var salt = new byte[saltLength];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            var ret = FromInput(password, salt, factor, hashLength);

            Clear(salt, 0, saltLength);

            return ret;
        }

        public static PasswordVendor FromInput(byte[] password, byte[] salt, int factor = Factor, int hashLength = HashLength)
        {
            byte[] hash;
            using (var rfc = new Rfc2898DeriveBytes(password, salt, (int)Math.Pow(2, factor)))
            {
                hash = rfc.GetBytes(hashLength);
            }

            var value = new byte[2 + salt.Length + hash.Length];
            value[0] = (byte)salt.Length;
            Buffer.BlockCopy(salt, 0, value, 1, salt.Length);
            value[salt.Length + 1] = (byte)factor;
            Buffer.BlockCopy(hash, 0, value, salt.Length + 2, hash.Length);

            Clear(hash, 0, hash.Length);

            return new PasswordVendor(value);
        }

        public static PasswordVendor FromStore(byte[] storeBytes, byte[] entropy = null)
        {
            entropy = entropy ?? Entropy;

            var value = ProtectedData.Unprotect(storeBytes, entropy, DataProtectionScope.CurrentUser);
            return new PasswordVendor(value);
        }

        public static PasswordVendor FromString(string str)
        {
            var parts = str.Split(new[] {"$"}, StringSplitOptions.RemoveEmptyEntries);
            if(!str.StartsWith($"${Prefix}$") || parts.Length != 4)
                throw new InvalidOperationException("PasswordVendor - Malformed string");

            if(parts[1] != Method)
                throw new NotImplementedException("PasswordVendor - Unsupported method");

            var data = Convert.FromBase64String(parts[2]);
            var check = Convert.FromBase64String(parts[3]);
            var sum = Checksum(data);

            if(check.Length != sum.Length)
                throw new InvalidOperationException("PasswordVendor - Malformed checksum");

            for (var i = 0; i < sum.Length; i++)
            {
                if(check[i] != sum[i])
                    throw new InvalidOperationException("PasswordVendor - Checksum failed");
            }

            return new PasswordVendor(data);
        }

        public byte[] ToStore(byte[] entropy = null)
        {
            entropy = entropy ?? Entropy;

            return ProtectedData.Protect(_pass, entropy, DataProtectionScope.CurrentUser);
        }

        public override string ToString()
        {
            return $"${Prefix}${Method}${Convert.ToBase64String(_pass)}${Convert.ToBase64String(Checksum(_pass))}";
        }

        public byte[] ToByteArray()
        {
            var ret = new byte[_pass.Length];
            Buffer.BlockCopy(_pass, 0, ret, 0, ret.Length);
            return ret;
        }

        public static byte[] ToByteArray(SecureString secureString, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;

            IntPtr bStr;
            int length;
            try
            {
                bStr = Marshal.SecureStringToBSTR(secureString);
                length = Marshal.ReadInt32(bStr, -4);
            }
            catch
            {
                return null;
            }

            var utf16Bytes = new byte[length];
            var utf16BytesPin = GCHandle.Alloc(utf16Bytes, GCHandleType.Pinned);

            try
            {
                Marshal.Copy(bStr, utf16Bytes, 0, length);
                Marshal.ZeroFreeBSTR(bStr);

                return Encoding.Convert(Encoding.Unicode, encoding, utf16Bytes);
            }
            catch
            {
                // ignored
            }
            finally
            {
                Clear(utf16Bytes, 0, utf16Bytes.Length);
                utf16BytesPin.Free();
            }

            return null;
        }

        private static void Clear(byte[] arr, int idx, int len)
        {
            for (var i = 0; i < len; i++)
            {
                arr[idx + i] = 0;
            }
        }

        private static byte[] Checksum(byte[] data)
        {
            byte sum1 = 0;
            byte sum2 = 0;
            var count = data.Length;
            int index;

            for (index = 0; index < count; ++index)
            {
                sum1 = (byte)((sum1 + data[index]) % 255);
                sum2 = (byte)((sum2 + sum1) % 255);
            }

            return new []{sum2, sum1};
        }
    }
}
