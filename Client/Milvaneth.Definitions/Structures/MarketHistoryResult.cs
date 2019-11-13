using MessagePack;
using System.Collections.Generic;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class MarketHistoryResult : IResult
    {
        [Key(0)]
        public int ItemId;
        [Key(1)]
        public List<MarketHistoryItem> HistoryItems;
    }
}
