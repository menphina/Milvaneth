using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class ConfigStore
    {
        [Key(0)]
        public ApiConfig Api { get; set; }
        [Key(1)]
        public ChecksumConfig Checksum { get; set; }
        [Key(2)]
        public GlobalConfig Global { get; set; }
    }
}
