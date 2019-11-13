using System;
using System.Collections.Generic;

namespace Milvaneth.Server.Models
{
    public partial class CharacterData
    {
        public CharacterData()
        {
            HistoryData = new HashSet<HistoryData>();
            ListingDataArtisan = new HashSet<ListingData>();
            ListingDataOwner = new HashSet<ListingData>();
            ListingDataReporter = new HashSet<ListingData>();
            OverviewData = new HashSet<OverviewData>();
            RetainerData = new HashSet<RetainerData>();
        }

        public long CharacterId { get; set; }
        public string CharacterName { get; set; }
        public long? ServiceId { get; set; }
        public long? AccountId { get; set; }
        public int HomeWorld { get; set; }
        public long[] RetainerList { get; set; }
        public short[] JobLevels { get; set; }
        public byte[] Inventory { get; set; }
        public int? GilHold { get; set; }

        public virtual AccountData Account { get; set; }
        public virtual ICollection<HistoryData> HistoryData { get; set; }
        public virtual ICollection<ListingData> ListingDataArtisan { get; set; }
        public virtual ICollection<ListingData> ListingDataOwner { get; set; }
        public virtual ICollection<ListingData> ListingDataReporter { get; set; }
        public virtual ICollection<OverviewData> OverviewData { get; set; }
        public virtual ICollection<RetainerData> RetainerData { get; set; }
    }
}
