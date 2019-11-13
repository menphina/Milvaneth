using Milvaneth.Server.Models;
using System;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Milvaneth.Server.Statics;

namespace Milvaneth.Server.Service
{
    public class AuditedApiKeySignService : IApiKeySignService
    {
        private readonly MilvanethDbContext _context;
        private readonly ITimeService _time;
        private readonly IAuthentication _auth;
        private readonly RNGCryptoServiceProvider _cryptoRandom;

        public AuditedApiKeySignService(MilvanethDbContext context, ITimeService time, IAuthentication auth)
        {
            _context = context;
            _time = time;
            _auth = auth;
            _cryptoRandom = new RNGCryptoServiceProvider();
        }

        public KeyStore Sign(KeyUsage usage, int quota, AccountData account, DateTime from, DateTime to)
        {
            var key = new byte[GlobalConfig.TOKEN_LENGTH];
            _cryptoRandom.GetBytes(key);

            if(usage.Usage == 0)
                throw new InvalidOperationException("Key Usage Not Recorded");

            //var keyStore = _context.KeyStore.CreateProxy();
            var keyStore = new KeyStore();
            {
                keyStore.Key = key;
                keyStore.HoldingAccountNavigation = account;
                keyStore.ValidFrom = from;
                keyStore.ValidUntil = to;
                keyStore.LastActive = _time.UtcNow;
                keyStore.UsageNavigation = usage;
                keyStore.ReuseCounter = -1;
                keyStore.Quota = quota;
            };

            _context.KeyStore.Add(keyStore);

            _context.SaveChanges();

            _auth.EnsureKey(keyStore, usage, GlobalOperation.GENERATE_APIKEY, 0, "Sign New Key", "N/A");

            return keyStore;
        }
    }
}
