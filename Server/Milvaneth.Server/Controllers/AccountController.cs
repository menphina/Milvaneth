using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Milvaneth.Common;
using Milvaneth.Common.Communication.Data;
using Milvaneth.Common.Communication.Login;
using Milvaneth.Common.Communication.Recovery;
using Milvaneth.Common.Communication.Register;
using Milvaneth.Server.Models;
using Milvaneth.Server.Service;
using Milvaneth.Server.Statics;
using Milvaneth.Server.Token;
using Serilog;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Milvaneth.Server.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/x-msgpack")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private IHttpContextAccessor _accessor;
        private ITimeService _time;
        private MilvanethDbContext _context;
        private IAuthentication _auth;
        private IPowService _pow;
        private ISrp6Service _srp;
        private IApiKeySignService _api;
        private ITokenSignService _token;
        private IVerifyMailService _mail;
        private IRepository _repo;
        private PrivilegeConfig _userPrivilege;
        private KeyUsage _changeToken;

        public AccountController(IHttpContextAccessor accessor, ITimeService time, MilvanethDbContext context, IAuthentication auth, IPowService pow, ISrp6Service srp, IApiKeySignService api, ITokenSignService token, IVerifyMailService mail, IRepository repo)
        {
            _accessor = accessor;
            _time = time;
            _context = context;
            _auth = auth;
            _pow = pow;
            _srp = srp;
            _api = api;
            _token = token;
            _mail = mail;
            _repo = repo;
            _userPrivilege = _context.PrivilegeConfig.Single(x => x.Name == "User");
            _changeToken = _context.KeyUsage.Single(x => x.Name == "Password Change Token");
        }

        [Route("create")]
        [HttpPost]
        public ActionResult<MilvanethProtocol> AccountCreate(MilvanethProtocol data)
        {
            if (!(data?.Data is RegisterForm form) || !form.Check())
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

            if (!_pow.Verify(form.ProofOfWork))
            {
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new ServerChallenge
                    {
                        Message = GlobalMessage.RATE_POW_REQUIRED,
                        ProofOfWork = _pow.Generate((byte)Math.Max(GlobalConfig.POW_SENSITIVE_OPERATION, _pow.Difficulty)),
                        ReportTime = _time.SafeNow,
                    }
                };
            }

            try
            {
                if (_context.AccountData.Any(x => x.AccountName == form.Username))
                {
                    return new MilvanethProtocol
                    {
                        Context = null,
                        Data = new ServerChallenge
                        {
                            Message = GlobalMessage.DATA_USERNAME_OCCUPIED,
                            ReportTime = _time.SafeNow,
                        }
                    };
                }

                //var accountData = _context.AccountData.CreateProxy();
                var accountData = new AccountData();
                {
                    accountData.AccountName = form.Username;
                    accountData.DisplayName = form.DisplayName;
                    accountData.Email = form.Email;
                    accountData.EmailConfirmed = false;
                    accountData.Salt = form.Salt;
                    accountData.Verifier = form.Verifier;
                    accountData.GroupParam = (short) form.GroupParam;
                    accountData.RegisterService = form.Service.ServiceId;
                    accountData.RelatedService = new long[] {form.Service.ServiceId};
                    accountData.PlayedCharacter = null;
                    accountData.Trace = form.Trace;
                    accountData.Karma = 0;
                    accountData.PrivilegeLevelNavigation = _userPrivilege;
                    accountData.SuspendUntil = null;
                    accountData.PasswordRetry = 0;
                    accountData.LastRetry = DateTime.UtcNow;
                }

                _context.AccountData.Add(accountData);

                _context.SaveChanges();

                _repo.Character.CommitRange(accountData.AccountId, form.Character.CharacterItems.Select(x => x.ToDb(accountData.RegisterService)), true);

                _auth.EnsureAccount(accountData, new PrivilegeConfig { Login = true }, GlobalOperation.ACCOUNT_CREATE, GlobalConfig.USER_INITIAL_KARMA,
                    "Registered new account via account/create", _accessor.GetIp());

                var session = _srp.DoServerResponse(accountData.AccountId, accountData.GroupParam, accountData.Verifier, out var token);

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
                Log.Error(e, "Error in ACCOUNT/CREATE");
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

        [Route("status")]
        [HttpGet]
        public ActionResult<MilvanethProtocol> AccountStatus([FromQuery] string token)
        {
            if (!_token.TryDecode(token, out var payload))
            {
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new AccountStatus
                    {
                        Message = GlobalMessage.OP_TOKEN_NOT_FOUND,
                        ReportTime = _time.SafeNow,
                    }
                };
            }

            try
            {
                if (payload.ValidTo < _time.UtcNow)
                {
                    return new MilvanethProtocol
                    {
                        Context = null,
                        Data = new AccountStatus
                        {
                            Message = GlobalMessage.OP_TOKEN_RENEW_REQUIRED,
                            ReportTime = _time.SafeNow,
                        }
                    };
                }

                _auth.EnsureToken(payload, TokenPurpose.AccessToken, GlobalOperation.ACCOUNT_STATUS, 0, out var account);

                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new AccountStatus
                    {
                        Message = GlobalMessage.OK_SUCCESS,
                        ReportTime = _time.SafeNow,
                        DisplayName = account.DisplayName,
                        Email = account.Email,
                        EstiKarma = (int) Helper.EstimateNumber(account.Karma, 3),
                        Username = account.AccountName
                    }
                };
            }
            catch (Exception e)
            {
                Log.Error(e, "Error in ACCOUNT/STATUS");
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new AccountStatus
                    {
                        Message = GlobalMessage.OP_INVALID,
                        ReportTime = _time.SafeNow,
                    }
                };
            }
        }

        [Route("update")]
        [HttpPost]
        public ActionResult AccountUpdate([FromQuery] string token, MilvanethProtocol data)
        {
            if (!_token.TryDecode(token, out var payload))
            {
                return StatusCode(401);
            }

            if (!(data?.Data is AccountUpdate update) || !update.Check())
            {
                return StatusCode(400);
            }

            try
            {
                if (payload.ValidTo < _time.UtcNow)
                {
                    return StatusCode(511);
                }

                _auth.EnsureToken(payload, TokenPurpose.AccessToken, GlobalOperation.ACCOUNT_STATUS, 0, out var account);

                account.Trace = account.Trace.Union(update.Trace).ToArray();
                account.DisplayName = update.DisplayName;

                _context.AccountData.Update(account);
                _context.SaveChanges();

                return StatusCode(200);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error in ACCOUNT/UPDATE");
                return StatusCode(500);
            }
        }

        [Route("recovery")]
        [HttpPost]
        public ActionResult<MilvanethProtocol> AccountRecovery(MilvanethProtocol data)
        {
            if (data?.Data is RecoveryEmail)
            {
                return AccountRecoveryEmail(data);
            }

            if (data?.Data is RecoveryGame)
            {
                return AccountRecoveryGame(data);
            }

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

        [Route("recoveryemail")]
        [HttpPost]
        public ActionResult<MilvanethProtocol> AccountRecoveryEmail(MilvanethProtocol data)
        {
            if (!(data?.Data is RecoveryEmail email) || !email.Check())
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
                var user = _context.AccountData.Include(x => x.PrivilegeLevelNavigation).SingleOrDefault(x => x.AccountName == email.Username);

                if (user == null)
                {
                    return new MilvanethProtocol
                    {
                        Context = null,
                        Data = new ServerResponse
                        {
                            Message = GlobalMessage.DATA_NO_SUCH_USER,
                            ReportTime = _time.SafeNow,
                        }
                    };
                }

                _auth.EnsureAccount(user, new PrivilegeConfig{AccountOperation = true}, GlobalOperation.ACCOUNT_RECOVERY_EMAIL, 0, "Recovery account via account/recovery with email", _accessor.GetIp());

                if (user.Email == null)
                {
                    return new MilvanethProtocol
                    {
                        Context = null,
                        Data = new ServerResponse
                        {
                            Message = GlobalMessage.DATA_NO_EMAIL_RECORDED,
                            ReportTime = _time.SafeNow,
                        }
                    };
                }

                var record = _context.EmailVerifyCode.OrderByDescending(x => x.SendTime).FirstOrDefault();

                if (string.IsNullOrEmpty(email.Code))
                {
                    if (record != null && record.SendTime.AddSeconds(GlobalConfig.ACCOUNT_VERIFY_CODE_COOLDOWN) > _time.UtcNow)
                    {
                        return new MilvanethProtocol
                        {
                            Context = null,
                            Data = new ServerResponse
                            {
                                Message = GlobalMessage.RATE_LIMIT,
                                ReportTime = _time.SafeNow,
                            }
                        };
                    }

                    if (user.Email.Equals(email.Email, StringComparison.InvariantCultureIgnoreCase))
                    {
                        string code;
                        if (record != null && record.ValidTo > _time.UtcNow.AddSeconds(3 * GlobalConfig.ACCOUNT_VERIFY_CODE_COOLDOWN))
                        {
                            code = record.Code;

                            record.SendTime = _time.UtcNow;

                            _context.EmailVerifyCode.Update(record);
                        }
                        else
                        {
                            using (var cryptoRng = new RNGCryptoServiceProvider())
                            {
                                var seed = new byte[5];
                                cryptoRng.GetBytes(seed);
                                code = Helper.ToCode(seed);
                            }

                            _context.EmailVerifyCode.Add(new EmailVerifyCode
                            {
                                AccountId = user.AccountId,
                                Email = user.Email,
                                FailedRetry = 0,
                                ValidTo = _time.UtcNow.AddSeconds(GlobalConfig.ACCOUNT_VERIFY_CODE_LIFE_TIME),
                                Code = code,
                                SendTime = _time.UtcNow
                            });
                        }

                        _context.SaveChanges();

                        _mail.SendCode(user.Email, user.DisplayName, code);
                    }
                    else
                    {
                        var rand = new Random();
                        // prevent timing attack
                        Thread.Sleep(1000 + rand.Next(3000));
                    }

                    return new MilvanethProtocol
                    {
                        Context = null,
                        Data = new ServerResponse
                        {
                            Message = GlobalMessage.OK_SUCCESS,
                            ReportTime = _time.SafeNow,
                        }
                    };
                }

                if (record == null || record.ValidTo < _time.UtcNow || record.FailedRetry >= 5)
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

                if (record.Code != email.Code.ToUpperInvariant())
                {
                    record.FailedRetry += 1;

                    _context.EmailVerifyCode.Update(record);

                    _context.SaveChanges();

                    return new MilvanethProtocol
                    {
                        Context = null,
                        Data = new ServerResponse
                        {
                            Message = GlobalMessage.DATA_INVALID_CAPTCHA,
                            ReportTime = _time.SafeNow,
                        }
                    };
                }

                var recovery = _api.Sign(_changeToken, 1, user, _time.UtcNow, _time.UtcNow.AddSeconds(GlobalConfig.TOKEN_ACCOUNT_LIFE_TIME));

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
                Log.Error(e, "Error in ACCOUNT/RECOVERYMAIL");
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

        [Route("recoverygame")]
        [HttpPost]
        public ActionResult<MilvanethProtocol> AccountRecoveryGame(MilvanethProtocol data)
        {
            if (!(data?.Data is RecoveryGame game) || !game.Check())
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

            if (!_pow.Verify(game.ProofOfWork))
            {
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new ServerResponse
                    {
                        Message = GlobalMessage.RATE_POW_REQUIRED,
                        AuthToken = _pow.Generate((byte)Math.Max(GlobalConfig.POW_SENSITIVE_OPERATION, _pow.Difficulty)),
                        ReportTime = _time.SafeNow,
                    }
                };
            }

            try
            {
                var user = _context.AccountData.Include(x => x.PrivilegeLevelNavigation).Single(x => x.AccountName == game.Username);

                _auth.EnsureAccount(user, new PrivilegeConfig { AccountOperation = true }, GlobalOperation.ACCOUNT_RECOVERY_GAME, 0, "Recovery account via account/recovery with game", _accessor.GetIp());

                var success = game.Trace.Count(t => user.Trace.Contains(t));

                if (!DataChecker.WeightedSuccess(success, game.Trace.Length, user.Trace.Length))
                {
                    return new MilvanethProtocol
                    {
                        Context = null,
                        Data = new ServerResponse
                        {
                            Message = GlobalMessage.DATA_TRACE_MISMATCH,
                            ReportTime = _time.SafeNow,
                        }
                    };
                }

                if (user.RegisterService != game.Service.ServiceId)
                {
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

                var recovery = _api.Sign(_changeToken, 1, user, _time.UtcNow, _time.UtcNow.AddSeconds(GlobalConfig.TOKEN_ACCOUNT_LIFE_TIME));

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
                Log.Error(e, "Error in ACCOUNT/RECOVERYGAME");
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