using Milvaneth.Common;
using Milvaneth.Common.Communication.Auth;
using Milvaneth.Common.Communication.Login;
using Milvaneth.Communication.Api;
using System;
using Milvaneth.Communication.Vendor;

namespace Milvaneth.Communication.Procedure
{
    public class ChangeProcedure : IProcedure
    {
        private string _username;
        private long[] _trace;
        private LoginProcedure _loginProcedure;
        private ServerResponse _serverResponse;
        private RecoveryProcedure _recoveryProcedure;

        public int Step1(string username, long[] trace)
        {
            if (CheckVendor.NotValidUsername(username) || CheckVendor.NotValidTrace(trace))
                return 03_0010;

            _username = username;
            _trace = trace;
            _loginProcedure = new LoginProcedure();
            return _loginProcedure.Step1(username, trace);
        }

        public int Step2(byte[] oldPassword)
        {
            if (CheckVendor.NotValidPassword(oldPassword))
                return 03_0010;

            var ret = _loginProcedure.Step2(oldPassword);
            _serverResponse = _loginProcedure.Step2GetResult();
            return ret;
        }

        public int Step3()
        {
            if (CheckVendor.NotValidResponse(_serverResponse))
                return 02_0008;

            var authRequest = new AuthRequest();

            authRequest.Username = _username;

            authRequest.AuthToken = _serverResponse.AuthToken;

            authRequest.ReportTime = DateTime.Now;

            var result = ApiCall.SessionChange.Call(null, new MilvanethProtocol { Context = null, Data = authRequest });

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

        public int Step4(byte[] newPassword)
        {
            _recoveryProcedure = new RecoveryProcedure();
            return _recoveryProcedure.Step3SetContext(_serverResponse, _username, newPassword, _trace);
        }

        public int Step5()
        {
            return _recoveryProcedure.Step4();
        }

        public int Step6()
        {
            return _recoveryProcedure.Step5();
        }

        public int Step7(out byte[] renewToken)
        {
            return _recoveryProcedure.Step6(out renewToken);
        }
    }
}
