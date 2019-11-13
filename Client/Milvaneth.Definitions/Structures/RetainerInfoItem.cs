using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class RetainerInfoItem
    {
        [Key(0)]
        public long RetainerId;
        [Key(1)]
        public byte RetainerOrder;
        [Key(2)]
        public byte ItemsInSell;
        [Key(3)]
        public byte RetainerLocation;
        [Key(4)]
        public int ListingDueDate;
        [Key(5)]
        public string RetainerName;
    }
}
