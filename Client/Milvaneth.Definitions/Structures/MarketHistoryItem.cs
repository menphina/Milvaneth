using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class MarketHistoryItem
    {
        [Key(0)]
        public int ItemId;
        [Key(1)]
        public int UnitPrice;
        [Key(2)]
        public int PurchaseTime;
        [Key(3)]
        public int Quantity;
        [Key(4)]
        public byte IsHq;
        [Key(5)]
        public byte Padding;
        [Key(6)]
        public byte OnMannequin;
        [Key(7)]
        public string BuyerName;
    }
}
