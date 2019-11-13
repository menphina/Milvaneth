using Microsoft.Extensions.Configuration;
using Milvaneth.Server.Models;
using Milvaneth.Server.Token;
using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Milvaneth.Server.Service
{
    public class AuditedTokenSignService : ITokenSignService
    {
        private MilvanethDbContext _context;
        private ITimeService _time;
        private TokenSigner _signer;

        public AuditedTokenSignService(MilvanethDbContext context, ITimeService time, IConfigurationRoot root)
        {
            _context = context;
            _time = time;
            _signer = new TokenSigner(GetTokenKey(root));
        }

        public string Sign(TokenPayload payload)
        {
            //var issue = _context.TokenIssueList.CreateProxy();
            var issue = new TokenIssueList();
            {
                issue.HoldingAccount = payload.AccountId;
                issue.Reason = (int) payload.Purpose;
                issue.IssueTime = _time.UtcNow;
                issue.ValidUntil = payload.ValidTo;
            };

            _context.TokenIssueList.Add(issue);

            _context.SaveChanges();

            payload.TokenId = issue.TokenSerial;

            return _signer.Encode(payload.ToPayloadBytes());
        }

        public TokenPayload Decode(string token)
        {
            if(!_signer.TryDecode(token, out var payloadBytes))
                throw new InvalidOperationException("Token Forged");

            return TokenPayload.FromPayloadBytes(payloadBytes);
        }

        public bool TryDecode(string token, out TokenPayload payload)
        {
            payload = null;

            if (!_signer.TryDecode(token, out var payloadBytes))
                return false;

            try
            {
                payload = TokenPayload.FromPayloadBytes(payloadBytes);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private byte[] GetTokenKey(IConfigurationRoot config)
        {
            var secret = config.GetSection("TokenSecret")?.GetValue("SecretKey", "");

            if (string.IsNullOrWhiteSpace(secret))
            {
                throw new InvalidOperationException(
                    "Could not find a token secret named 'SecretKey'.");
            }

            using (HashAlgorithm hash = new SHA256CryptoServiceProvider())
            {
                hash.Initialize();
                return hash.ComputeHash(Encoding.UTF8.GetBytes(secret));
            }
        }
    }
}
