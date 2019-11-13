using System;
using System.Collections.Generic;

namespace Milvaneth.Server.Models
{
    public partial class RetainerData
    {
        public RetainerData()
        {
            ListingData = new HashSet<ListingData>();
        }

        public long RetainerId { get; set; }
        public string RetainerName { get; set; }
        public long Character { get; set; }
        public byte[] Inventory { get; set; }
        public int World { get; set; }
        public short Location { get; set; }

        public virtual CharacterData CharacterNavigation { get; set; }
        public virtual ICollection<ListingData> ListingData { get; set; }
    }
}
