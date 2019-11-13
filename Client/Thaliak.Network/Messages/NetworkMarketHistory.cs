using Milvaneth.Common;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkMarketHistory : NetworkMessageProcessor
    {
        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkMarketHistory);
        }

        public new static unsafe MarketHistoryResult Consume(byte[] data, int offset)
        {
            fixed (byte* raw = &data[offset])
            {
                return (*(NetworkMarketHistoryRaw*) raw).Spawn(data, offset);
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NetworkMarketHistoryRaw : INetworkMessageBase<MarketHistoryResult>
    {
        [FieldOffset(0)]
        public int ItemId;

        public MarketHistoryResult Spawn(byte[] data, int offset)
        {
            const int itemSize = 52;
            const int itemCount = 20;
            var items = new List<MarketHistoryItem>(itemCount);
            for (var i = 0; i < itemCount; i++)
            {
                fixed (byte* p = &data[offset + 4 + i * itemSize])
                {
                    var item = (*(NetworkMarketHistoryItemRaw*) p).Spawn(data, offset + 4 + i * itemSize);
                    if(item.ItemId != 0)
                        items.Add(item);
                }
            }

            return new MarketHistoryResult
            {
                HistoryItems = items,
                ItemId = this.ItemId,
            };
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NetworkMarketHistoryItemRaw : INetworkMessageBase<MarketHistoryItem>
    {
        [FieldOffset(0)]
        public int ItemId;

        [FieldOffset(4)]
        public int UnitPrice;

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

        public MarketHistoryItem Spawn(byte[] data, int offset)
        {
            return new MarketHistoryItem
            {
                UnitPrice = this.UnitPrice,
                PurchaseTime = this.PurchaseTime,
                Quantity = this.Quantity,
                IsHq = this.IsHq,
                Padding = this.Padding,
                OnMannequin = this.OnMannequin,
                BuyerName = Helper.ToUtf8String(data, offset, 19, 33),
                ItemId = this.ItemId,
            };
        }
    }
}
