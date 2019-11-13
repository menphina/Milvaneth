using Milvaneth.Common;

namespace Thaliak.Writer
{
    class Cnf
    {
        internal static void SetConfigStore(ConfigStore conf, string runChecksumAgainstPath)
        {
            if (!string.IsNullOrWhiteSpace(runChecksumAgainstPath))
            {
                conf.Checksum = ChecksumConfig.PerformHashing(runChecksumAgainstPath);
            }
            else
            {
                conf.Checksum = null;
            }
            var path = Helper.GetMilFilePathRaw("apidef.pack");
            var ser = new Serializer<ConfigStore>(path, "");
            ser.Save(conf);
        }
    }
}
