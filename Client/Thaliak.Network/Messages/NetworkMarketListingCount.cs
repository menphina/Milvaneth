using Milvaneth.Common;
using System.Runtime.InteropServices;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkMarketListingCount : NetworkMessageProcessor, IResult
    {
        public int ItemId;
        public int Unknown1;
        public short RequestId;
        public short Quantity;
        public int Unknown2;

        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkMarketListingCount);
        }

        public new static unsafe NetworkMarketListingCount Consume(byte[] data, int offset)
        {
            fixed (byte* raw = &data[offset])
            {
                return (*(NetworkMarketListingCountRaw*) raw).Spawn(data, offset);
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NetworkMarketListingCountRaw : INetworkMessageBase<NetworkMarketListingCount>
    {
        [FieldOffset(0)]
        public int ItemId;

        [FieldOffset(4)]
        public int Unknown1;

        [FieldOffset(8)]
        public short RequestId;

        [FieldOffset(10)]
        public short Quantity; // This field is big-endian integer

        [FieldOffset(12)]
        public int Unknown2;

        public NetworkMarketListingCount Spawn(byte[] data, int offset)
        {
            return new NetworkMarketListingCount
            {
                ItemId = this.ItemId,
                Unknown1 = this.Unknown1,
                RequestId = this.RequestId,
                Quantity = (short)((this.Quantity << 8) | (this.Quantity >> 8)),
                Unknown2 = this.Unknown2,
            };
        }
    }
}
