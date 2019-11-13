using Milvaneth.Common;
using Milvaneth.Common.Communication.Auth;
using Milvaneth.Communication.Api;
using Milvaneth.Communication.Vendor;
using System;

namespace Milvaneth.Communication.Procedure
{
    public class RenewProcedure : IProcedure
    {
        private AuthResponse _authResponse;

        public int Step1(string username, byte[] renewToken)
        {
            if (CheckVendor.NotValidRenewToken(renewToken))
                return 03_0009;

            var authRenew = new AuthRenew();

            authRenew.Username = username;

            authRenew.RenewToken = renewToken;

            authRenew.ReportTime = DateTime.Now;

            var result = ApiCall.SessionRenew.Call(null, new MilvanethProtocol { Context = null, Data = authRenew });

            if (!(result.Data is AuthResponse ar))
            {
                return 02_0007;
            }

            if (!CheckVendor.NotValidResponse(ar))
            {
                _authResponse = ar;
            }

            return ar.Message;
        }

        public int Step2(out byte[] renewToken)
        {
            renewToken = null;

            if (CheckVendor.NotValidResponse(_authResponse))
                return 02_0008;

            renewToken = _authResponse.RenewToken;
            ApiVendor.SetToken(_authResponse.SessionToken);

            return _authResponse.Message;
        }
    }
}
