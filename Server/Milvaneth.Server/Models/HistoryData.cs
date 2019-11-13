using System;
using System.Collections.Generic;

namespace Milvaneth.Server.Models
{
    public partial class HistoryData
    {
        public long RecordId { get; set; }
        public Guid BucketId { get; set; }
        public DateTime ReportTime { get; set; }
        public int World { get; set; }
        public long ReporterId { get; set; }
        public int ItemId { get; set; }
        public int UnitPrice { get; set; }
        public long PurchaseTime { get; set; }
        public int Quantity { get; set; }
        public bool IsHq { get; set; }
        public bool Padding { get; set; }
        public bool OnMannequin { get; set; }
        public string BuyerName { get; set; }

        public virtual CharacterData Reporter { get; set; }
    }
}
