using Milvaneth.Communication.Api;
using Milvaneth.Communication.Vendor;

namespace Milvaneth.Communication.Procedure.Auth
{
    public class LogoutProcedure : IProcedure
    {
        public int Step1()
        {
            if (!ApiVendor.HasToken())
                return 02_0008;

            var result = ApiCall.SessionLogout.Call(null, null);

            return 00_0000;
        }
    }
}
