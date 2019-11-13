using Milvaneth.Common;
using System.Runtime.InteropServices;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkRetainerSummary : NetworkMessageProcessor, IResult
    {
        public long RetainerId; //?
        public byte RetainerOrder;
        public byte ItemHold; // BC
        public int GilHold;
        public byte ItemInSell;
        public byte RetainerLocation;
        public int ListingDueDate; // unix, due may = 0?
        public string RetainerName; // 32b

        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkRetainerSummary);
        }

        public new static unsafe NetworkRetainerSummary Consume(byte[] data, int offset)
        {
            fixed (byte* raw = &data[offset])
            {
                return (*(NetworkRetainerSummaryRaw*)raw).Spawn(data, offset);
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NetworkRetainerSummaryRaw : INetworkMessageBase<NetworkRetainerSummary>
    {
        [FieldOffset(8)]
        public long RetainerId;

        [FieldOffset(16)]
        public byte RetainerOrder;

        [FieldOffset(17)]
        public byte ItemHold;

        [FieldOffset(20)]
        public int GilHold;

        [FieldOffset(24)]
        public byte ItemInSell;

        [FieldOffset(25)]
        public byte RetainerLocation;

        [FieldOffset(28)]
        public int ListingDueDate;

        public NetworkRetainerSummary Spawn(byte[] data, int offset)
        {
            return new NetworkRetainerSummary
            {
                RetainerId = this.RetainerId,
                RetainerOrder = this.RetainerOrder,
                ItemHold = this.ItemHold,
                GilHold = this.GilHold,
                ItemInSell = this.ItemInSell,
                RetainerLocation = this.RetainerLocation,
                ListingDueDate = this.ListingDueDate,
                RetainerName = Helper.ToUtf8String(data, offset, 41, 32),
            };
        }
    }
}
