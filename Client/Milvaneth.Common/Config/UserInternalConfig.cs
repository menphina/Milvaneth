using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class UserInternalConfig : IUserConfig
    {
        [Key(0)]
        public string InternalUsername; // Not property

        [Key(1)]
        public byte[] InternalRenewToken; // Not property

        [Key(2)]
        public string InternalOverlayAssemblyPath { get; set; }

        [Key(3)]
        public string InternalPerferredApiEntry { get; set; }
    }
}
