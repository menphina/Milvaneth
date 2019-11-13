using MessagePack;
using System.Collections.Generic;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class MarketOverviewResult : IResult
    {
        [Key(0)]
        public List<MarketOverviewItem> ResultItems;
        [Key(1)]
        public int ItemIndexEnd;
        [Key(2)]
        public int Padding;
        [Key(3)]
        public int ItemIndexStart;
        [Key(4)]
        public int RequestId;
    }
}
