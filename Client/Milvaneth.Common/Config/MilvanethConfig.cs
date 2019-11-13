using System;

namespace Milvaneth.Common
{
    public class MilvanethConfig
    {
        private static readonly Lazy<MilvanethConfig> lazy = new Lazy<MilvanethConfig>(() => new MilvanethConfig());
        public static ConfigStore Store => lazy.Value._configStore;

        private readonly ConfigStore _configStore;

        private MilvanethConfig()
        {
            var path = Helper.GetMilFilePathRaw("apidef.pack");
            var serializer = new Serializer<ConfigStore>(path, "");
            _configStore = serializer.Load();
        }
    }
}
