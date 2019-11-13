using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Milvaneth.Common;
using Milvaneth.Common.Communication.Auth;
using Milvaneth.Common.Communication.Login;
using Milvaneth.Server.Models;
using Milvaneth.Server.Service;
using Milvaneth.Server.Statics;
using Milvaneth.Server.Token;
using Serilog;
using System;
using System.Linq;

namespace Milvaneth.Server.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/x-msgpack")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private IHttpContextAccessor _accessor;
        private ITimeService _time;
        private MilvanethDbContext _context;
        private IAuthentication _auth;
        private IApiKeySignService _api;
        private ITokenSignService _token;
        private KeyUsage _changeToken;
        private KeyUsage _renewToken;

        public SessionController(IHttpContextAccessor accessor, ITimeService time, MilvanethDbContext context, IAuthentication auth, IApiKeySignService api, ITokenSignService token)
        {
            _accessor = accessor;
            _time = time;
            _context = context;
            _auth = auth;
            _api = api;
            _token = token;
            _changeToken = _context.KeyUsage.Single(x => x.Name == "Password Change Token");
            _renewToken = _context.KeyUsage.Single(x => x.Name == "Renew Token");
        }

        [Route("create")]
        [HttpPost]
        public ActionResult<MilvanethProtocol> SessionCreate(MilvanethProtocol data)
        {
            if (!(data?.Data is AuthRequest request) || !request.Check())
            {
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new AuthResponse
                    {
                        Message = GlobalMessage.DATA_INVALID_INPUT,
                        ReportTime = _time.SafeNow,
                    }
                };
            }

            try
            {
                var key = _context.KeyStore
                    .Include(x => x.HoldingAccountNavigation)
                        .ThenInclude(x => x.PrivilegeLevelNavigation)
                    .Include(x => x.UsageNavigation)
                    .Single(x => x.Key.SequenceEqual(request.AuthToken));

                _auth.EnsureKey(key, new KeyUsage { CreateSession = true }, GlobalOperation.SESSION_CREATE, 0, "Create session via session/create",
                    _accessor.GetIp());

                var account = key.HoldingAccountNavigation;

                _auth.EnsureAccount(account, new PrivilegeConfig {AccessData = true}, GlobalOperation.SESSION_CREATE, 0,
                    "Create session via session/create", _accessor.GetIp());

                var renew = _api.Sign(_renewToken, 1, account, _time.UtcNow,
                    _time.UtcNow.AddSeconds(GlobalConfig.TOKEN_RENEW_LIFE_TIME));

                var access = _token.Sign(new TokenPayload(_time.UtcNow.AddSeconds(GlobalConfig.TOKEN_DATA_LIFE_TIME),
                    account.AccountId, TokenPurpose.AccessToken, renew.KeyId));

                _context.SaveChanges();

                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new AuthResponse
                    {
                        Message = GlobalMessage.OK_SUCCESS,
                        ReportTime = _time.SafeNow,
                        RenewToken = renew.Key,
                        SessionToken = access
                    }
                };
            }
            catch (Exception e)
            {
                Log.Error(e, "Error in SESSION/CREATE");
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new ServerChallenge
                    {
                        Message = GlobalMessage.OP_INVALID,
                        ReportTime = _time.SafeNow,
                    }
                };
            }
        }

        [Route("change")]
        [HttpPost]
        public ActionResult<MilvanethProtocol> SessionChange(MilvanethProtocol data)
        {
            if (!(data?.Data is AuthRequest request) || !request.Check())
            {
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new ServerResponse
                    {
                        Message = GlobalMessage.DATA_INVALID_INPUT,
                        ReportTime = _time.SafeNow,
                    }
                };
            }

            try
            {
                var key = _context.KeyStore
                    .Include(x => x.HoldingAccountNavigation)
                        .ThenInclude(x => x.PrivilegeLevelNavigation)
                    .Include(x => x.UsageNavigation)
                    .Single(x => x.Key.SequenceEqual(request.AuthToken));

                _auth.EnsureKey(key, new KeyUsage { GetChangeToken = true }, GlobalOperation.SESSION_CHANGE, 0, "Create change token via session/change",
                    _accessor.GetIp());

                var account = key.HoldingAccountNavigation;

                _auth.EnsureAccount(account, new PrivilegeConfig { AccountOperation = true }, GlobalOperation.SESSION_CHANGE, 0,
                    "Create change token via session/change", _accessor.GetIp());

                var recovery = _api.Sign(_changeToken, 1, account, _time.UtcNow, _time.UtcNow.AddSeconds(GlobalConfig.TOKEN_ACCOUNT_LIFE_TIME));

                _context.SaveChanges();

                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new ServerResponse
                    {
                        Message = GlobalMessage.OK_SUCCESS,
                        ReportTime = _time.SafeNow,
                        AuthToken = recovery.Key,
                    }
                };
            }
            catch (Exception e)
            {
                Log.Error(e, "Error in SESSION/CHANGE");
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new ServerResponse
                    {
                        Message = GlobalMessage.OP_INVALID,
                        ReportTime = _time.SafeNow,
                    }
                };
            }
        }

        [Route("renew")]
        [HttpPost]
        public ActionResult<MilvanethProtocol> SessionRenew([FromQuery] string token, MilvanethProtocol data)
        {
            if (!(data?.Data is AuthRenew renew) || !renew.Check())
            {
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new AuthResponse
                    {
                        Message = GlobalMessage.DATA_INVALID_INPUT,
                        ReportTime = _time.SafeNow,
                    }
                };
            }

            try
            {
                _token.TryDecode(token, out var payload);

                var key = _context.KeyStore
                    .Include(x => x.HoldingAccountNavigation)
                    .ThenInclude(x => x.PrivilegeLevelNavigation)
                    .Include(x => x.UsageNavigation)
                    .Single(x => x.Key.SequenceEqual(renew.RenewToken));

                _auth.EnsureKey(key, new KeyUsage {RenewSession = true, CreateSession = payload == null},
                    GlobalOperation.SESSION_RENEW, 0, "Renew session via session/renew",
                    _accessor.GetIp());

                var account = key.HoldingAccountNavigation;

                _auth.EnsureAccount(account, new PrivilegeConfig { AccessData = true }, GlobalOperation.SESSION_RENEW, 0,
                    "Create session via session/renew", _accessor.GetIp());

                var newRenew = _api.Sign(_renewToken, 1, account, _time.UtcNow,
                    _time.UtcNow.AddSeconds(GlobalConfig.TOKEN_RENEW_LIFE_TIME));

                var access = _token.Sign(new TokenPayload(_time.UtcNow.AddSeconds(GlobalConfig.TOKEN_DATA_LIFE_TIME),
                    account.AccountId, TokenPurpose.AccessToken, newRenew.KeyId));

                if (payload != null)
                {
                    _context.TokenRevocationList.Add(new TokenRevocationList
                    {
                        Reason = GlobalOperation.SESSION_RENEW,
                        RevokeSince = _time.UtcNow,
                        TokenSerial = payload.TokenId,
                    });
                }

                _context.SaveChanges();

                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new AuthResponse
                    {
                        Message = GlobalMessage.OK_SUCCESS,
                        ReportTime = _time.SafeNow,
                        RenewToken = newRenew.Key,
                        SessionToken = access
                    }
                };
            }
            catch (Exception e)
            {
                Log.Error(e, "Error in SESSION/RENEW");
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new ServerChallenge
                    {
                        Message = GlobalMessage.OP_INVALID,
                        ReportTime = _time.SafeNow,
                    }
                };
            }
        }

        [Route("logout")]
        [HttpGet]
        public ActionResult SessionLogout([FromQuery] string token)
        {
            try
            {
                _token.TryDecode(token, out var payload);

                _auth.EnsureToken(payload, TokenPurpose.AccessToken, GlobalOperation.SESSION_LOGOUT, 0, out var account);

                if (payload.RelatedKey != -1)
                {
                    var key = _context.KeyStore.Single(x => x.KeyId == payload.RelatedKey);

                    if (key != null &&
                        key.ReuseCounter != key.Quota &&
                        key.ValidUntil > _time.UtcNow)
                    {
                        key.ReuseCounter = key.Quota;

                        _context.KeyStore.Update(key);

                        _context.ApiLog.Add(new ApiLog
                        {
                            ReportTime = _time.UtcNow,
                            AccountId = account.AccountId,
                            KeyId = key.KeyId,
                            Operation = GlobalOperation.SESSION_LOGOUT,
                            Detail = "Deactivate via session/logout",
                            IpAddress = _accessor.GetIp()
                        });
                    }
                }

                _context.TokenRevocationList.Add(new TokenRevocationList
                {
                    Reason = GlobalOperation.SESSION_LOGOUT,
                    RevokeSince = _time.UtcNow,
                    TokenSerial = payload.TokenId,
                });

                _context.SaveChanges();

                return StatusCode(200);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error in SESSION/LOGOUT");
                return StatusCode(500);
            }
        }
    }
}