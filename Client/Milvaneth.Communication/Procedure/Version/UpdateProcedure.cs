using Milvaneth.Common;
using Milvaneth.Common.Communication.Version;
using Milvaneth.Communication.Api;
using Milvaneth.Communication.Vendor;

namespace Milvaneth.Communication.Procedure.Version
{
    public class UpdateProcedure : IProcedure
    {
        private VersionInfo _versionInfo;

        public int Step1(out bool needUpdate, out VersionInfo info)
        {
            needUpdate = false;
            info = null;

            var result = ApiCall.VersionCurrent.Call($"{MilvanethConfig.Store.Global.MilVersion},{MilvanethConfig.Store.Global.DataVersion},{MilvanethConfig.Store.Global.GameVersion}", null);

            if (!(result.Data is VersionInfo vi))
                return 02_0007;

            if (CheckVendor.NotValidResponse(vi))
                return vi.Message;

            needUpdate = vi.Message == 00_0004;

            if (needUpdate)
            {
                info = vi;
                _versionInfo = vi;
            }

            return 00_0000;
        }

        public int Step2(out VersionDownload download)
        {
            download = null;

            if (CheckVendor.NotValidResponse(_versionInfo) || _versionInfo.Message != 00_0004)
                return 02_0008;

            var result = ApiCall.VersionDownload.Call(_versionInfo.EligibleBundleKey, null);

            if (!(result.Data is VersionDownload vd))
                return 02_0007;

            if (CheckVendor.NotValidResponse(vd))
                return vd.Message;

            vd.Argument = UpdateVendor.FormatArgumentString(vd);
            download = vd;

            return 00_0000;
        }
    }
}
