using Milvaneth.Common;
using Milvaneth.Common.Communication.Login;
using Milvaneth.Common.Communication.Recovery;
using Milvaneth.Communication.Api;
using Milvaneth.Communication.Vendor;
using System;

namespace Milvaneth.Communication.Procedure
{
    public class RecoveryProcedure : IProcedure
    {
        private string _username;
        private string _email;
        private byte[] _password;
        private long[] _trace;
        private ServerResponse _serverResponse;
        private LoginProcedure _loginProcedure;

        public int Step1(string username, string email, long[] trace)
        {
            if (CheckVendor.NotValidUsername(username) || 
                CheckVendor.NotValidEmail(email) ||
                CheckVendor.NotValidTrace(trace))
                return 03_0010;

            _username = username;
            _email = email;
            _trace = trace;
            var recoveryEmail = new RecoveryEmail();

            recoveryEmail.Username = username;

            recoveryEmail.Email = email;

            recoveryEmail.Trace = trace;

            recoveryEmail.ReportTime = DateTime.Now;

            var result = ApiCall.AccountRecoveryEmail.Call(null, new MilvanethProtocol { Context = null, Data = recoveryEmail });

            if (!(result.Data is ServerResponse sr))
            {
                return 02_0007;
            }

            if (!CheckVendor.NotValidResponse(sr))
            {
                _serverResponse = sr;
            }

            return sr.Message;
        }

        public int Step1(string username, LobbyServiceResult service, LobbyCharacterResult character, long[] trace)
        {
            if (CheckVendor.NotValidUsername(username) ||
                CheckVendor.NotValidService(service) ||
                CheckVendor.NotValidCharacter(character) ||
                CheckVendor.NotValidTrace(trace))
                return 03_0010;

            _username = username;
            var recoveryGame = new RecoveryGame();

            recoveryGame.Username = username;

            recoveryGame.Service = service;

            recoveryGame.Character = character;

            recoveryGame.Trace = trace;

            recoveryGame.ReportTime = DateTime.Now;

            for (var i = 0; i < 3; i++)
            {
                var result = ApiCall.AccountRecoveryGame.Call(null, new MilvanethProtocol { Context = null, Data = recoveryGame });

                if (!(result.Data is ServerResponse sr))
                {
                    return 02_0007;
                }

                if (!CheckVendor.NotValidResponse(sr))
                {
                    _serverResponse = sr;
                }

                if (sr.Message != 01_0003) // retry with pow
                {
                    return sr.Message;
                }

                recoveryGame.ProofOfWork = ProofOfWorkVendor.CalculateProofOfWork(sr.AuthToken);
            }

            return 01_0002;
        }

        public int Step2(string code)
        {
            if (CheckVendor.NotValidCode(code))
                return 03_0011;

            if (CheckVendor.NotValidResponse(_serverResponse))
                return 02_0008;

            if (_email == null) return 00_0000;

            var recoveryEmail = new RecoveryEmail();

            recoveryEmail.Username = _username;

            recoveryEmail.Email = _email;

            recoveryEmail.Trace = _trace;

            recoveryEmail.Code = code;

            recoveryEmail.ReportTime = DateTime.Now;

            var result = ApiCall.AccountRecoveryEmail.Call(null, new MilvanethProtocol { Context = null, Data = recoveryEmail });

            if (!(result.Data is ServerResponse sr))
            {
                return 02_0007;
            }

            if (!CheckVendor.NotValidResponse(sr))
            {
                _serverResponse = sr;
            }

            return sr.Message;
        }

        public int Step3(byte[] newPassword, string email)
        {
            if (CheckVendor.NotValidPassword(newPassword))
                return 03_0007;

            if (CheckVendor.NotValidEmail(email))
                return 03_0005;

            if (CheckVendor.NotValidResponse(_serverResponse))
                return 02_0008;

            _password = newPassword;
            var recoveryRequest = new RecoveryRequest();

            recoveryRequest.Email = email;

            recoveryRequest.Verifier = Srp6Vendor.Srp6Init(_username, _password, out var salt).ToByteArray();

            recoveryRequest.Salt = salt;

            recoveryRequest.GroupParam = Srp6Vendor.BitLength;

            recoveryRequest.OperationToken = _serverResponse.AuthToken;

            recoveryRequest.ReportTime = DateTime.Now;

            var result = ApiCall.AuthReset.Call(null, new MilvanethProtocol { Context = null, Data = recoveryRequest });

            if (!(result.Data is ServerResponse sr))
            {
                return 02_0007;
            }

            if (!CheckVendor.NotValidResponse(sr))
            {
                _serverResponse = sr;
            }

            return sr.Message;
        }

        public int Step4()
        {
            _loginProcedure = new LoginProcedure();
            return _loginProcedure.Step1(_username, _trace);
        }

        public int Step5()
        {
            return _loginProcedure.Step2(_password);
        }

        public int Step6(out byte[] renewToken)
        {
            return _loginProcedure.Step3(out renewToken);
        }

        internal int Step3SetContext(ServerResponse sr, string username, byte[] newPassword, long[] trace)
        {
            _username = username;
            _serverResponse = sr;
            _trace = trace;
            _email = null;
            _loginProcedure = null;
            return Step3(newPassword, _email);
        }
    }
}
