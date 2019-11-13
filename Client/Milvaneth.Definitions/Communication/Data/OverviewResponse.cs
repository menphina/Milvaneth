using MessagePack;
using Milvaneth.Common;
using System.Collections.Generic;

namespace Milvaneth.Definitions.Communication.Data
{
    [MessagePackObject]
    public class OverviewResponse : IMilvanethData, IMilvanethResponse
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public int Message { get; set; }
        [Key(2)]
        public int PartId { get; set; }
        [Key(3)]
        public int EstiTotalParts { get; set; }
        [Key(4)]
        public bool FinalPart { get; set; }
        [Key(5)]
        public List<OverviewResponseItem> Results;
    }

    [MessagePackObject]
    public class OverviewResponseItem
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public int World { get; set; }
        [Key(2)]
        public int ItemId;
        [Key(3)]
        public short OpenListing;
        [Key(4)]
        public short Demand;
    }
}
