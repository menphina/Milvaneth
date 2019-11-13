using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Milvaneth.Common;
using Milvaneth.Common.Communication.Login;
using Milvaneth.Common.Communication.Recovery;
using Milvaneth.Server.Models;
using Milvaneth.Server.Service;
using Milvaneth.Server.Statics;
using Serilog;
using System;
using System.Linq;

namespace Milvaneth.Server.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/x-msgpack")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private IHttpContextAccessor _accessor;
        private ITimeService _time;
        private MilvanethDbContext _context;
        private IAuthentication _auth;
        private IPowService _pow;
        private ISrp6Service _srp;
        private IApiKeySignService _api;
        private KeyUsage _authToken;

        public AuthController(IHttpContextAccessor accessor, ITimeService time, MilvanethDbContext context, IAuthentication auth, IPowService pow, ISrp6Service srp, IApiKeySignService api)
        {
            _accessor = accessor;
            _time = time;
            _context = context;
            _auth = auth;
            _pow = pow;
            _srp = srp;
            _api = api;
            _authToken = _context.KeyUsage.Single(x => x.Name == "Auth Token");
        }

        [Route("start")]
        [HttpPost]
        public ActionResult<MilvanethProtocol> AuthStart(MilvanethProtocol data)
        {
            if (!(data?.Data is ClientChallenge challenge) || !challenge.Check())
            {
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new ServerChallenge
                    {
                        Message = GlobalMessage.DATA_INVALID_INPUT,
                        ReportTime = _time.SafeNow,
                    }
                };
            }

            if (!_pow.Verify(challenge.ProofOfWork) && _pow.ConditionalGenerate(out var requirement))
            {
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new ServerChallenge
                    {
                        Message = GlobalMessage.RATE_POW_REQUIRED,
                        ProofOfWork = requirement,
                        ReportTime = _time.SafeNow,
                    }
                };
            }

            try
            {
                var accountData = _context.AccountData.Include(x => x.PrivilegeLevelNavigation).SingleOrDefault(x => x.AccountName == challenge.Username);

                if (accountData == null)
                {
                    return new MilvanethProtocol
                    {
                        Context = null,
                        Data = new ServerChallenge
                        {
                            Message = GlobalMessage.DATA_NO_SUCH_USER,
                            ReportTime = _time.SafeNow,
                        }
                    };
                }

                if (accountData.PrivilegeLevel == GlobalConfig.ACCOUNT_BLOCKED_LEVEL)
                {
                    return new MilvanethProtocol
                    {
                        Context = null,
                        Data = new ServerChallenge
                        {
                            Message = GlobalMessage.OP_SANCTION,
                            ReportTime = _time.SafeNow,
                        }
                    };
                }

                if (accountData.HasSuspended() && accountData.SuspendUntil >= _time.UtcNow)
                {
                    return new MilvanethProtocol
                    {
                        Context = null,
                        Data = new ServerChallenge
                        {
                            Message = GlobalMessage.OP_ACCOUNT_SUSPENDED,
                            ReportTime = _time.SafeNow,
                        }
                    };
                }

                if (accountData.PasswordRetry > GlobalConfig.ACCOUNT_PASSWORD_RETRY_TOLERANCE && accountData.LastRetry.HasValue &&
                    (_time.UtcNow - accountData.LastRetry.Value).Seconds < GlobalConfig.ACCOUNT_PASSWORD_RETRY_COOLDOWN)
                {
                    return new MilvanethProtocol
                    {
                        Context = null,
                        Data = new ServerChallenge
                        {
                            Message = GlobalMessage.OP_PASSWORD_RETRY_TOO_MUCH,
                            ReportTime = _time.SafeNow,
                        }
                    };
                }

                _auth.EnsureAccount(accountData, new PrivilegeConfig { Login = true }, GlobalOperation.AUTH_START, 0,
                    "Start login via auth/start", _accessor.GetIp());

                var session = _srp.DoServerResponse(accountData.AccountId, accountData.GroupParam, accountData.Verifier, out var token);

                _context.AccountData.Update(accountData);

                _context.SaveChanges();

                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new ServerChallenge
                    {
                        GroupParam = accountData.GroupParam,
                        Message = GlobalMessage.OK_SUCCESS,
                        ReportTime = _time.SafeNow,
                        Salt = accountData.Salt,
                        ServerToken = token,
                        SessionId = session
                    }
                };
            }
            catch (Exception e)
            {
                Log.Error(e, "Error in AUTH/START");
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

        [Route("finish")]
        [HttpPost]
        public ActionResult<MilvanethProtocol> AuthFinish(MilvanethProtocol data)
        {
            if (!(data?.Data is ClientResponse response) || !response.Check())
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
                if (!_srp.DoServerValidate(response.SessionId, response.ClientToken, response.ClientEvidence, out var accountId))
                {
                    if (accountId != 0)
                    {
                        var accountData = _context.AccountData.Single(x => x.AccountId == accountId);

                        if (accountData.PasswordRetry > GlobalConfig.ACCOUNT_PASSWORD_RETRY_TOLERANCE && accountData.LastRetry.HasValue &&
                            (_time.UtcNow - accountData.LastRetry.Value).Seconds < GlobalConfig.ACCOUNT_PASSWORD_RETRY_COOLDOWN)
                        {
                            return new MilvanethProtocol
                            {
                                Context = null,
                                Data = new ServerResponse
                                {
                                    Message = GlobalMessage.OP_PASSWORD_RETRY_TOO_MUCH,
                                    ReportTime = _time.SafeNow,
                                }
                            };
                        }

                        if (accountData.PasswordRetry == null)
                        {
                            accountData.PasswordRetry = 1;
                        }
                        accountData.PasswordRetry += 1;
                        accountData.LastRetry = _time.UtcNow;

                        _context.AccountData.Update(accountData);

                        _context.SaveChanges();
                    }

                    return new MilvanethProtocol
                    {
                        Context = null,
                        Data = new ServerResponse
                        {
                            Message = GlobalMessage.DATA_RECORD_MISMATCH,
                            ReportTime = _time.SafeNow,
                        }
                    };
                }

                var account = _context.AccountData.Include(x => x.PrivilegeLevelNavigation).Single(x => x.AccountId == accountId);

                if (account.PasswordRetry > GlobalConfig.ACCOUNT_PASSWORD_RETRY_TOLERANCE && account.LastRetry.HasValue &&
                    (_time.UtcNow - account.LastRetry.Value).Seconds < GlobalConfig.ACCOUNT_PASSWORD_RETRY_COOLDOWN)
                {
                    return new MilvanethProtocol
                    {
                        Context = null,
                        Data = new ServerResponse
                        {
                            Message = GlobalMessage.OP_PASSWORD_RETRY_TOO_MUCH,
                            ReportTime = _time.SafeNow,
                        }
                    };
                }

                account.PasswordRetry = 0;

                _context.AccountData.Update(account);

                _context.AccountLog.Add(new AccountLog
                {
                    ReportTime = _time.UtcNow,
                    AccountId = account.AccountId,
                    Message = GlobalOperation.AUTH_FINISH,
                    Detail = "Finish login via auth/finish",
                    IpAddress = _accessor.GetIp()
                });

                _context.SaveChanges();

                var key =_api.Sign(_authToken, 1, account, _time.UtcNow, _time.UtcNow.AddSeconds(GlobalConfig.TOKEN_ACCOUNT_LIFE_TIME));

                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new ServerResponse
                    {
                        Message = GlobalMessage.OK_SUCCESS,
                        ReportTime = _time.SafeNow,
                        AuthToken = key.Key
                    }
                };
            }
            catch (Exception e)
            {
                Log.Error(e, "Error in AUTH/FINISH");
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

        [Route("reset")]
        [HttpPost]
        public ActionResult<MilvanethProtocol> AuthReset(MilvanethProtocol data)
        {
            if (!(data?.Data is RecoveryRequest request) || !request.Check())
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
                    .Single(x => x.Key.SequenceEqual(request.OperationToken));

                _auth.EnsureKey(key, new KeyUsage {ChangePassword = true}, GlobalOperation.AUTH_RESET, 0, "Change password via auth/reset",
                    _accessor.GetIp());

                var account = key.HoldingAccountNavigation;

                if (!string.IsNullOrWhiteSpace(request.Email))
                {
                    account.Email = request.Email;
                }

                account.GroupParam = (short)request.GroupParam;
                account.Salt = request.Salt;
                account.Verifier = request.Verifier;

                var auth = _api.Sign(_authToken, 1, account, _time.UtcNow, _time.UtcNow.AddSeconds(GlobalConfig.TOKEN_ACCOUNT_LIFE_TIME));

                _context.AccountData.Update(account);

                _context.SaveChanges();

                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new ServerResponse
                    {
                        Message = GlobalMessage.OK_SUCCESS,
                        ReportTime = _time.SafeNow,
                        AuthToken = auth.Key
                    }
                };
            }
            catch (Exception e)
            {
                Log.Error(e, "Error in AUTH/RESET");
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
    }
}