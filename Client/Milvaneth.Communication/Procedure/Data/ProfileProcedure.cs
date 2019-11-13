using System;
using Milvaneth.Common;
using Milvaneth.Common.Communication.Data;
using Milvaneth.Communication.Api;
using Milvaneth.Communication.Vendor;

namespace Milvaneth.Communication.Procedure
{
    public class ProfileProcedure : IProcedure
    {
        public int Step1(string displayName, long[] trace, byte[] additional)
        {
            if (CheckVendor.NotValidDisplayName(displayName) || CheckVendor.NotValidTrace(trace) || !ApiVendor.HasToken())
                return 02_0008;

            var au = new AccountUpdate();
            au.DisplayName = displayName;
            au.Trace = trace;
            au.AdditionalData = additional;
            au.ReportTime = DateTime.Now;

            var result = ApiCall.AccountUpdate.Call(null, new MilvanethProtocol {Context = null, Data = au});

            return 00_0000;
        }

        public int Step2(out AccountStatus status)
        {
            status = null;

            if (!ApiVendor.HasToken())
                return 02_0008;

            var result = ApiCall.AccountStatus.Call(null, null);

            if (!(result.Data is AccountStatus ac))
                return 02_0007;

            if (CheckVendor.NotValidResponse(ac))
                return ac.Message;

            status = ac;
            return 00_0000;
        }
    }
}
