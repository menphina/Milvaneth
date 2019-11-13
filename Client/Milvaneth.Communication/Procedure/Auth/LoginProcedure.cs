using Milvaneth.Common;
using Milvaneth.Common.Communication.Auth;
using Milvaneth.Common.Communication.Login;
using Milvaneth.Communication.Api;
using Milvaneth.Communication.Vendor;
using Org.BouncyCastle.Math;
using System;

namespace Milvaneth.Communication.Procedure
{
    public class LoginProcedure : IProcedure
    {
        private string _username;
        private ServerChallenge _serverChallenge;
        private ServerResponse _serverResponse;

        public int Step1(string username, long[] trace)
        {
            var clientChallenge = new ClientChallenge();

            if (CheckVendor.NotValidUsername(username) || CheckVendor.NotValidTrace(trace))
                return 03_0010;

            clientChallenge.Trace = trace;

            _username = username;
            clientChallenge.Username = username;

            clientChallenge.ReportTime = DateTime.Now;

            for (var i = 0; i < 3; i++)
            {
                var result = ApiCall.AuthStart.Call(null, new MilvanethProtocol { Context = null, Data = clientChallenge });

                if (!(result.Data is ServerChallenge sc))
                {
                    return 02_0007;
                }

                if (!CheckVendor.NotValidResponse(sc))
                {
                    _serverChallenge = sc;
                }

                if (sc.Message != 01_0003) // retry with pow
                {
                    return sc.Message;
                }

                clientChallenge.SessionId = sc.SessionId;

                clientChallenge.ProofOfWork = ProofOfWorkVendor.CalculateProofOfWork(sc.ProofOfWork);
            }

            return 01_0002;
        }

        public int Step2(byte[] password)
        {
            if (CheckVendor.NotValidPassword(password))
                return 03_0010;

            if (CheckVendor.NotValidResponse(_serverChallenge))
                return 02_0008;

            var clientResponse = new ClientResponse();

            clientResponse.SessionId = _serverChallenge.SessionId;

            clientResponse.ClientEvidence = Srp6Vendor.Srp6Response(_username, password, _serverChallenge.Salt,
                new BigInteger(_serverChallenge.ServerToken), out var token).ToByteArray();

            clientResponse.ClientToken = token.ToByteArray();

            clientResponse.ReportTime = DateTime.Now;

            var result = ApiCall.AuthFinish.Call(null, new MilvanethProtocol { Context = null, Data = clientResponse });

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

        public int Step3(out byte[] renewToken)
        {
            renewToken = null;

            if (CheckVendor.NotValidResponse(_serverResponse))
                return 02_0008;

            var authRequest = new AuthRequest();

            authRequest.Username = _username;

            authRequest.AuthToken = _serverResponse.AuthToken;

            authRequest.ReportTime = DateTime.Now;

            var result = ApiCall.SessionCreate.Call(null, new MilvanethProtocol { Context = null, Data = authRequest });

            if (!(result.Data is AuthResponse ar))
            {
                return 02_0007;
            }

            if (!CheckVendor.NotValidResponse(ar))
            {
                renewToken = ar.RenewToken;
                ApiVendor.SetToken(ar.SessionToken);
            }

            return ar.Message;
        }

        internal int Step2SetContext(ServerChallenge sc, string username, byte[] password)
        {
            _username = username;
            _serverChallenge = sc;
            _serverResponse = null;
            return Step2(password);
        }

        internal ServerResponse Step2GetResult()
        {
            return _serverResponse;
        }
    }
}
