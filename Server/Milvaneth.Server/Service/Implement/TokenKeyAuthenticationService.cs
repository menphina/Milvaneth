using Microsoft.EntityFrameworkCore;
using Milvaneth.Server.Models;
using Milvaneth.Server.Statics;
using Milvaneth.Server.Token;
using System;
using System.Linq;

namespace Milvaneth.Server.Service
{
    public class TokenKeyAuthenticationService : IAuthentication
    {
        private readonly MilvanethDbContext _context;
        private readonly ITimeService _time;

        public TokenKeyAuthenticationService(MilvanethDbContext context, ITimeService time)
        {
            _context = context;
            _time = time;
        }

        public void EnsureAccount(AccountData account, PrivilegeConfig usage, int operation, int karma, string message, string ip)
        {
            if (account == null)
                throw new InvalidOperationException("Account Not Found");

            if (!usage.IsSatisfied(account.PrivilegeLevelNavigation))
                throw new InvalidOperationException("Account Insufficient Privilege");

            if (account.PrivilegeLevel == GlobalConfig.ACCOUNT_BLOCKED_LEVEL)
                throw new InvalidOperationException("Account Blacklisted");

            if (account.HasSuspended() && account.SuspendUntil >= _time.UtcNow)
                throw new InvalidOperationException("Account Suspended");

            if (!account.PrivilegeLevelNavigation.IgnoreKarma && karma != 0)
            {
                var before = account.Karma;
                var after = account.Karma + karma;

                if (after < 0 && GlobalConfig.USER_ENABLE_KARMA)
                    throw new InvalidOperationException("Account Insufficient Karma");

                account.Karma = after;

                _context.AccountData.Update(account);

                _context.KarmaLog.Add(new KarmaLog
                {
                    ReportTime = _time.UtcNow,
                    AccountId = account.AccountId,
                    Reason = operation,
                    Before = before,
                    After = after
                });
            }

            _context.AccountLog.Add(new AccountLog
            {
                ReportTime = _time.UtcNow,
                AccountId = account.AccountId,
                Message = operation,
                Detail = message,
                IpAddress = ip
            });

            _context.SaveChanges();
        }

        public void EnsureToken(TokenPayload token, TokenPurpose usage, int operation, int karma, out AccountData account)
        {
            account = null;

            if (token == null)
                throw new InvalidOperationException("Token Not Found");

            if (token.ValidTo < _time.UtcNow)
                throw new InvalidOperationException("Token Outdated");

            if (!usage.IsSatisfied(token.Purpose))
                throw new InvalidOperationException("Token Insufficient Privilege");

            if (_context.TokenRevocationList.Any(x => x.TokenSerial == token.TokenId))
                throw new InvalidOperationException("Token Revoked");

            account = _context.AccountData.Include(x => x.PrivilegeLevelNavigation).Single(x => x.AccountId == token.AccountId);

            if (account == null)
                throw new InvalidOperationException("Token Account Not Found");

            if (account.PrivilegeLevel == GlobalConfig.ACCOUNT_BLOCKED_LEVEL)
                throw new InvalidOperationException("Token Account Blacklisted");

            if (account.SuspendUntil != null && account.SuspendUntil >= _time.UtcNow)
                throw new InvalidOperationException("Token Account Suspended");

            if (!account.PrivilegeLevelNavigation.IgnoreKarma && karma != 0)
            {
                var before = account.Karma;
                var after = account.Karma + karma;

                if (after < 0 && GlobalConfig.USER_ENABLE_KARMA)
                    throw new InvalidOperationException("Token Insufficient Karma");

                account.Karma = after;

                _context.AccountData.Update(account);

                _context.KarmaLog.Add(new KarmaLog
                {
                    ReportTime = _time.UtcNow,
                    AccountId = account.AccountId,
                    Reason = operation,
                    Before = before,
                    After = after
                });
            }

            _context.SaveChanges();
        }

        public void EnsureKey(KeyStore keyInfo, KeyUsage usage, int operation, int karma, string message, string ip)
        {
            if (keyInfo == null)
                throw new InvalidOperationException("Api Key Not Found");

            if (keyInfo.ValidUntil < _time.UtcNow || keyInfo.ValidFrom > _time.UtcNow)
                throw new InvalidOperationException("Api Key Outdated");

            if (!usage.IsSatisfied(keyInfo.UsageNavigation))
                throw new InvalidOperationException("Api Key Insufficient Privilege");

            if (keyInfo.ReuseCounter >= keyInfo.Quota && keyInfo.Quota != -1)
                throw new InvalidOperationException("Api Key Out of Quota");

            if (keyInfo.HoldingAccountNavigation.HasSuspended() && keyInfo.HoldingAccountNavigation.SuspendUntil >= _time.UtcNow)
                throw new InvalidOperationException("Api Key Account Suspended");

            if (keyInfo.HoldingAccountNavigation.PrivilegeLevel == GlobalConfig.ACCOUNT_BLOCKED_LEVEL)
                throw new InvalidOperationException("Api Key Account Blacklisted");

            if (!keyInfo.HoldingAccountNavigation.PrivilegeLevelNavigation.IgnoreKarma && karma != 0)
            {
                var before = keyInfo.HoldingAccountNavigation.Karma;
                var after = keyInfo.HoldingAccountNavigation.Karma + karma;

                if (after < 0 && GlobalConfig.USER_ENABLE_KARMA)
                    throw new InvalidOperationException("Api Key Insufficient Karma");

                keyInfo.HoldingAccountNavigation.Karma = after;

                _context.KeyStore.Update(keyInfo);

                _context.KarmaLog.Add(new KarmaLog
                {
                    ReportTime = _time.UtcNow,
                    AccountId = keyInfo.HoldingAccount,
                    Reason = operation,
                    Before = before,
                    After = after
                });
            }

            keyInfo.LastActive = _time.UtcNow;
            keyInfo.ReuseCounter++;

            _context.KeyStore.Update(keyInfo);

            _context.ApiLog.Add(new ApiLog
            {
                ReportTime = _time.UtcNow,
                AccountId = keyInfo.HoldingAccount,
                KeyId = keyInfo.KeyId,
                Operation = operation,
                Detail = message,
                IpAddress = ip
            });

            _context.SaveChanges();
        }
    }
}
