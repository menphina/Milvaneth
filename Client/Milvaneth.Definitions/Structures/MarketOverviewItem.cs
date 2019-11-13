using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public struct MarketOverviewItem
    {
        [Key(0)]
        public int ItemId;
        [Key(1)]
        public short OpenListing;
        [Key(2)]
        public short Demand;
    }
}
