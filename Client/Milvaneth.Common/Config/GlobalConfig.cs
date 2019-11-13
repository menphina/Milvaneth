using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class GlobalConfig
    {
        [Key(0)]
        public int GameVersion;
        [IgnoreMember]
        public int MilVersion = MilvanethVersion.VersionNumber;
        [Key(2)]
        public int DataVersion;
        [Key(3)]
        public string ProjectUrl;
        [Key(4)]
        public string CustomMessage;
    }
}
