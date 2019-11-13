using MessagePack;
using System.Collections.Generic;

namespace Milvaneth.Common.Communication.Data
{
    [MessagePackObject]
    public class PackedResultBundle : IMilvanethData, IMilvanethResponse
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public int Message { get; set; }
        [Key(2)]
        public int ItemId { get; set; }
        [Key(3)]
        public List<ListingResponseItem> Listings { get; set; }
        [Key(4)]
        public List<HistoryResponseItem> Histories { get; set; }
    }

    [MessagePackObject]
    public class ListingResponseItem
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public int WorldId { get; set; }
        [Key(2)]
        public MarketListingItem RawItem { get; set; }
    }

    [MessagePackObject]
    public class HistoryResponseItem
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public int WorldId { get; set; }
        [Key(2)]
        public MarketHistoryItem RawItem { get; set; }
    }
}