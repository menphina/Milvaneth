using Milvaneth.Common;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkRetainerHistory : NetworkMessageProcessor
    {
        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkRetainerHistory);
        }

        public new static unsafe RetainerHistoryResult Consume(byte[] data, int offset)
        {
            fixed (byte* raw = &data[offset])
            {
                return (*(NetworkRetainerHistoryRaw*) raw).Spawn(data, offset);
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NetworkRetainerHistoryRaw : INetworkMessageBase<RetainerHistoryResult>
    {
        [FieldOffset(0)]
        public long RetainerId;

        public RetainerHistoryResult Spawn(byte[] data, int offset)
        {
            const int itemSize = 52;
            const int itemCount = 20;
            var items = new List<RetainerHistoryItem>(itemCount);
            for (var i = 0; i < itemCount; i++)
            {
                fixed (byte* p = &data[offset + 8 + i * itemSize])
                {
                    var item = (*(NetworkRetainerHistoryItemRaw*) p).Spawn(data, offset + 8 + i * itemSize);
                    if (item.ItemId != 0)
                        items.Add(item);
                }
            }

            return new RetainerHistoryResult
            {
                RetainerId = this.RetainerId,
                HistoryItems = items,
            };
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NetworkRetainerHistoryItemRaw : INetworkMessageBase<RetainerHistoryItem>
    {
        [FieldOffset(0)]
        public int ItemId;

        [FieldOffset(4)]
        public int TotalPrice;

        [FieldOffset(8)]
        public int PurchaseTime;

        [FieldOffset(12)]
        public int Quantity;

        [FieldOffset(16)]
        public byte IsHq;

        [FieldOffset(17)]
        public byte Padding;

        [FieldOffset(18)]
        public byte OnMannequin;

        public RetainerHistoryItem Spawn(byte[] data, int offset)
        {
            fixed (byte* p = &data[offset])
            {
                return new RetainerHistoryItem
                {
                    ItemId = this.ItemId,
                    TotalPrice = this.TotalPrice,
                    PurchaseTime = this.PurchaseTime,
                    Quantity = this.Quantity,
                    IsHq = this.IsHq,
                    Padding = this.Padding,
                    OnMannequin = this.OnMannequin,
                    BuyerName = Helper.ToUtf8String(data, offset, 19, 33),
                };
            }
        }
    }
}
