using Milvaneth.Common;
using Milvaneth.Common.Communication.Data;
using Milvaneth.Communication.Api;
using Milvaneth.Communication.Vendor;
using Milvaneth.Definitions.Communication.Data;
using System;
using System.Linq;

namespace Milvaneth.Communication.Procedure
{
    public class ExchangeProcedure : IProcedure
    {
        public int Step1(MilvanethProtocol mp)
        {
            if (!ApiVendor.HasToken())
                return 02_0008;

            if (mp == null) // as we have karma, this is not too serious
                return 00_0001;

            if (CheckVendor.NotValidData(mp))
                return 02_0008;

            var result = ApiCall.DataUpload.Call(null, mp);

            return 00_0000;
        }

        public int Step2(int itemid, out PackedResultBundle res)
        {
            res = null;

            if (CheckVendor.NotValidItemId(itemid) || !ApiVendor.HasToken())
                return 02_0008;

            var result = ApiCall.DataItem.Call(itemid.ToString(), null);

            if (!(result.Data is PackedResultBundle pr))
                return 02_0007;

            if (CheckVendor.NotValidResponse(pr))
                return pr.Message;

            res = pr;
            return 00_0000;
        }

        public int Step2(int[] itemid, int partid, out OverviewResponse res)
        {
            res = null;

            if (itemid.Any(CheckVendor.NotValidItemId) || !ApiVendor.HasToken())
                return 02_0008;

            var or = new OverviewRequest();
            or.QueryItems = itemid;
            or.ReportTime = DateTime.Now;

            var result = ApiCall.DataOverview.Call(partid.ToString(), new MilvanethProtocol {Context = null, Data = or});

            if (!(result.Data is OverviewResponse pr))
                return 02_0007;

            if (CheckVendor.NotValidResponse(pr))
                return pr.Message;

            res = pr;
            return 00_0000;
        }
    }
}
