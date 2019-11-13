using Milvaneth.Common;
using Milvaneth.Common.Communication.Login;
using Milvaneth.Common.Communication.Register;
using Milvaneth.Communication.Api;
using Milvaneth.Communication.Vendor;
using System;

namespace Milvaneth.Communication.Procedure
{
    public class RegisterProcedure : IProcedure
    {
        private string _username;
        private byte[] _password;
        private LoginProcedure _loginProcedure;
        private ServerChallenge _serverChallenge;

        public int Step1(LobbyServiceResult service, LobbyCharacterResult character, string username, string displayName, string email, byte[] password, long[] trace)
        {
            var registerForm = new RegisterForm();

            if (CheckVendor.NotValidService(service))
                return 03_0000;

            registerForm.Service = service;

            if (CheckVendor.NotValidCharacter(character))
                return 03_0000;

            registerForm.Character = character;

            if (CheckVendor.NotValidTrace(trace))
                return 03_0008;

            registerForm.Trace = trace;

            // 4-16个字符，可使用英文、数字和下划线，必须以字母开头
            if (CheckVendor.NotValidUsername(username))
                return 03_0001;

            registerForm.Username = username;

            // 2-12个字符，可使用中英文、数字和下划线
            if (CheckVendor.NotValidDisplayName(displayName))
                return 03_0003;

            registerForm.DisplayName = displayName;

            if (CheckVendor.NotValidEmail(email))
                return 03_0005;

            registerForm.Email = email;

            // 4个字符以上的中文或8个字符以上的数字和字母
            if (CheckVendor.NotValidPassword(password))
                return 03_0007;

            _username = username;
            _password = password;

            registerForm.Verifier = Srp6Vendor.Srp6Init(username, password, out var salt).ToByteArray();

            registerForm.Salt = salt;

            registerForm.GroupParam = Srp6Vendor.BitLength;

            registerForm.ReportTime = DateTime.Now;

            for (var i = 0; i < 3; i++)
            {
                var result = ApiCall.AccountCreate.Call(null, new MilvanethProtocol {Context = null, Data = registerForm});

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

                registerForm.SessionId = sc.SessionId;

                registerForm.ProofOfWork = ProofOfWorkVendor.CalculateProofOfWork(sc.ProofOfWork);
            }

            return 01_0002;
        }

        public int Step2()
        {
            _loginProcedure = new LoginProcedure();
            return _loginProcedure.Step2SetContext(_serverChallenge, _username, _password);
        }

        public int Step3(out byte[] renewToken)
        {
            return _loginProcedure.Step3(out renewToken);
        }
    }
}
