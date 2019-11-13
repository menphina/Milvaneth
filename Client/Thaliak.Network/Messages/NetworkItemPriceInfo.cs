using Milvaneth.Common;
using System.Runtime.InteropServices;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkItemPriceInfo : NetworkMessageProcessor, IResult
    {
        public int ContainerSequence;
        public short ContainerId;
        public short Unknown1;
        public short ContainerSlot;
        public short Unknown2;
        public int Unknown3;
        public int UnitPrice;

        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkItemPriceInfo);
        }

        public new static unsafe NetworkItemPriceInfo Consume(byte[] data, int offset)
        {
            fixed (byte* raw = &data[offset])
            {
                return (*(NetworkItemPriceInfoRaw*)raw).Spawn(data, offset);
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NetworkItemPriceInfoRaw : INetworkMessageBase<NetworkItemPriceInfo>
    {
        [FieldOffset(0)]
        public int ContainerSequence;

        [FieldOffset(4)]
        public short ContainerId;

        [FieldOffset(6)]
        public short Unknown1;

        [FieldOffset(8)]
        public short ContainerSlot;

        [FieldOffset(10)]
        public short Unknown2;

        [FieldOffset(12)]
        public int Unknown3;

        [FieldOffset(16)]
        public int UnitPrice;

        public NetworkItemPriceInfo Spawn(byte[] data, int offset)
        {
            return new NetworkItemPriceInfo
            {
                ContainerSequence = this.ContainerSequence,
                ContainerId = this.ContainerId,
                Unknown1 = this.Unknown1,
                ContainerSlot = this.ContainerSlot,
                Unknown2 = this.Unknown2,
                Unknown3 = this.Unknown3,
                UnitPrice = this.UnitPrice,
            };
        }
    }
}
