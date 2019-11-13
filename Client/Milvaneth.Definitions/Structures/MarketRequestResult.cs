using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class MarketRequestResult : IResult
    {
        [Key(0)]
        public int ItemId;
    }
}