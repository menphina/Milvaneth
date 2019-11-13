using System;
using System.Collections.Generic;

namespace Milvaneth.Server.Models
{
    public partial class OverviewData
    {
        public long RecordId { get; set; }
        public Guid BucketId { get; set; }
        public DateTime ReportTime { get; set; }
        public int World { get; set; }
        public long ReporterId { get; set; }
        public int ItemId { get; set; }
        public short OpenListing { get; set; }
        public short Demand { get; set; }

        public virtual CharacterData Reporter { get; set; }
    }
}
