using MessagePack;
using System.Collections.Generic;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class MarketListingResult : IResult
    {
        [Key(0)]
        public List<MarketListingItem> ListingItems;
        [IgnoreMember]
        public byte ListingIndexEnd;
        [IgnoreMember]
        public byte ListingIndexStart;
        [Key(1)]
        public short RequestId;
        [IgnoreMember]
        public short Padding;
    }
}
