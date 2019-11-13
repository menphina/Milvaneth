using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class RetainerUpdateItem
    {
        [Key(0)]
        public long RetainerId;
        [Key(1)]
        public InventoryContainerId ContainerId;
        [Key(2)]
        public int ContainerSlot;
        [Key(3)]
        public int NewPrice;
        [Key(4)]
        public int OldPrice;
        [Key(5)]
        public InventoryItem ItemInfo;
        [Key(6)]
        public bool IsRemove;
    }
}