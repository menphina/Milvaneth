using System;
using System.Collections.Generic;

namespace Milvaneth.Server.Models
{
    public partial class ListingData
    {
        public long RecordId { get; set; }
        public Guid BucketId { get; set; }
        public DateTime ReportTime { get; set; }
        public int World { get; set; }
        public long ReporterId { get; set; }
        public long ListingId { get; set; }
        public long RetainerId { get; set; }
        public long OwnerId { get; set; }
        public long ArtisanId { get; set; }
        public int UnitPrice { get; set; }
        public int TotalTax { get; set; }
        public int Quantity { get; set; }
        public int ItemId { get; set; }
        public long UpdateTime { get; set; }
        public short ContainerId { get; set; }
        public short SlotId { get; set; }
        public short Condition { get; set; }
        public short SpiritBond { get; set; }
        public short Materia1 { get; set; }
        public short Materia2 { get; set; }
        public short Materia3 { get; set; }
        public short Materia4 { get; set; }
        public short Materia5 { get; set; }
        public string RetainerName { get; set; }
        public string PlayerName { get; set; }
        public bool IsHq { get; set; }
        public short MateriaCount { get; set; }
        public bool OnMannequin { get; set; }
        public short RetainerLoc { get; set; }
        public short DyeId { get; set; }

        public virtual CharacterData Artisan { get; set; }
        public virtual CharacterData Owner { get; set; }
        public virtual CharacterData Reporter { get; set; }
        public virtual RetainerData Retainer { get; set; }
    }
}
