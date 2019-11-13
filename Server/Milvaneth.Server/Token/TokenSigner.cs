using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace Milvaneth.Server.Token
{
    public class TokenSigner : IDisposable
    {
        private const int KeyBytes = 32;
        private const int NonceBytes = 12;
        private const int MagicBytes = 3;
        private const int MacBytes = 16;
        private const int HeaderBytes = NonceBytes + MagicBytes;
        private readonly byte[] _magicBytes = {49, 83, 126}; // MVN-

        private readonly KeyParameter _key;
        private readonly GcmBlockCipher _gcmEngine;
        private readonly RNGCryptoServiceProvider _rng;

        public TokenSigner(byte[] key)
        {
            if (KeyBytes != key.Length)
            {
                throw new ArgumentException($"Key must be exactly {KeyBytes} bytes.");
            }

            _key = new KeyParameter(key);
            _gcmEngine = new GcmBlockCipher(new AesEngine());
            _rng = new RNGCryptoServiceProvider();
        }

        public void Dispose()
        {
            try { _rng.Dispose(); } catch { /* ignored */ }
        }

        public string Encode(byte[] payload)
        {
            var nonceBytes = new byte[NonceBytes];
            _rng.GetBytes(nonceBytes);

            var headerBytes = new byte[HeaderBytes];
            Buffer.BlockCopy(_magicBytes, 0, headerBytes, 0, MagicBytes);
            Buffer.BlockCopy(nonceBytes, 0, headerBytes, MagicBytes, NonceBytes);

            var aead = new AeadParameters(_key, MacBytes * 8, nonceBytes, headerBytes);
            _gcmEngine.Init(true, aead);

            var cipherTextBytes = new byte[_gcmEngine.GetOutputSize(payload.Length)];

            var pos = _gcmEngine.ProcessBytes(payload, 0, payload.Length, cipherTextBytes, 0);
            _gcmEngine.DoFinal(cipherTextBytes, pos);

            var result = new byte[HeaderBytes + cipherTextBytes.Length];
            Buffer.BlockCopy(headerBytes, 0, result, 0, HeaderBytes);
            Buffer.BlockCopy(cipherTextBytes, 0, result, HeaderBytes, cipherTextBytes.Length);

            return ToCompatBase64Url(result);
        }

        public byte[] Decode(string token)
        {
            var result = FromCompatBase64Url(token);

            var headerBytes = new byte[HeaderBytes];
            Buffer.BlockCopy(result, 0, headerBytes, 0, HeaderBytes);

            var magicBytes = new byte[MagicBytes];
            Buffer.BlockCopy(headerBytes, 0, magicBytes, 0, MagicBytes);

            if (!magicBytes.SequenceEqual(this._magicBytes))
                throw new InvalidOperationException("Token Invalid Magic");

            var nonceBytes = new byte[NonceBytes];
            Buffer.BlockCopy(headerBytes, MagicBytes, nonceBytes, 0, NonceBytes);

            var cipherTextBytes = new byte[result.Length - HeaderBytes];
            Buffer.BlockCopy(result, headerBytes.Length, cipherTextBytes, 0, cipherTextBytes.Length);

            var aead = new AeadParameters(_key, MacBytes * 8, nonceBytes, headerBytes);
            _gcmEngine.Init(false, aead);

            var payload = new byte[_gcmEngine.GetOutputSize(cipherTextBytes.Length)];

            var pos = _gcmEngine.ProcessBytes(cipherTextBytes, 0, cipherTextBytes.Length, payload, 0);
            _gcmEngine.DoFinal(payload, pos);

            return payload;
        }

        public bool TryDecode(string token, out byte[] payload)
        {
            payload = null;

            if (string.IsNullOrWhiteSpace(token))
                return false;

            if (token.Length % 4 == 1)
                return false;

            if (!token.StartsWith("MVN-"))
                return false;

            try
            {
                payload = Decode(token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string ToCompatBase64Url(byte[] input)
        {
            return Base64UrlEncoder.Encode(input);
        }

        private byte[] FromCompatBase64Url(string input)
        {
            return Base64UrlEncoder.DecodeBytes(input);
        }
    }
}
