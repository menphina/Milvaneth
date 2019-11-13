using Milvaneth.Common;
using System.Runtime.InteropServices;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkInventoryModify : NetworkMessageProcessor, IResult
    {
        public byte CommandType;
        public InventoryContainerId FromContainer;
        public byte FromSlot;
        public InventoryContainerId ToContainer;
        public byte ToSlot;
        public int SplitCount;

        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkInventoryModify);
        }

        public new static unsafe NetworkInventoryModify Consume(byte[] data, int offset)
        {
            fixed (byte* raw = &data[offset])
            {
                var dat = *(NetworkInventoryModifyRaw*)raw;

                return new NetworkInventoryModify
                {
                    CommandType = dat.CommandType,
                    FromContainer = dat.FromContainer,
                    FromSlot = dat.FromSlot,
                    ToContainer = dat.ToContainer,
                    ToSlot = dat.ToSlot,
                    SplitCount = dat.SplitCount,
                };
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NetworkInventoryModifyRaw
    {
        [FieldOffset(0x0)]
        public int SequenceId;

        [FieldOffset(0x4)]
        public byte CommandType;

        [FieldOffset(0xC)]
        public InventoryContainerId FromContainer;

        [FieldOffset(0x10)]
        public byte FromSlot;

        [FieldOffset(0x20)]
        public InventoryContainerId ToContainer;

        [FieldOffset(0x24)]
        public byte ToSlot;

        [FieldOffset(0x28)]
        public int SplitCount;
    }
}