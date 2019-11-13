using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class MarketListingItem
    {
        [Key(0)]
        public long ListingId;
        [Key(1)]
        public long RetainerId;
        [Key(2)]
        public long OwnerId;
        [Key(3)]
        public long ArtisanId;
        [Key(4)]
        public int UnitPrice;
        [Key(5)]
        public int TotalTax;
        [Key(6)]
        public int Quantity;
        [Key(7)]
        public int ItemId;
        [Key(8)]
        public int UpdateTime;
        [Key(9)]
        public short ContainerId;
        [Key(10)]
        public short SlotId;
        [Key(11)]
        public short Condition;
        [Key(12)]
        public short SpiritBond;
        [Key(13)]
        public short Materia1;
        [Key(14)]
        public short Materia2;
        [Key(15)]
        public short Materia3;
        [Key(16)]
        public short Materia4;
        [Key(17)]
        public short Materia5;
        [Key(18)]
        public short Unknown1;
        [Key(19)]
        public int Unknown2;
        [Key(20)]
        public string RetainerName;
        [Key(21)]
        public string PlayerName;
        [Key(22)]
        public byte IsHq;
        [Key(23)]
        public byte MateriaCount;
        [Key(24)]
        public byte OnMannequin;
        [Key(25)]
        public byte RetainerLocation;
        [Key(26)]
        public short DyeId;
        [Key(27)]
        public short Unknown3;
        [Key(28)]
        public int Unknown4;
        [Key(29)]
        public string ArtisanName;
    }
}
