using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class MilvanethProtocol
    {
        [Key(0)]
        public MilvanethContext Context;
        [Key(1)]
        public IMilvanethData Data;
    }
}
