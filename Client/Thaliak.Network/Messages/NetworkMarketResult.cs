using Milvaneth.Common;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkMarketResult : NetworkMessageProcessor
    {
        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkMarketResult);
        }

        public new static unsafe MarketOverviewResult Consume(byte[] data, int offset)
        {
            fixed (byte* raw = &data[offset])
            {
                return (*(NetworkMarketResultRaw*) raw).Spawn(data, offset);
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NetworkMarketResultRaw : INetworkMessageBase<MarketOverviewResult>
    {
        [FieldOffset(160)]
        public int ItemIndexEnd;

        [FieldOffset(164)]
        public int Padding;

        [FieldOffset(168)]
        public int ItemIndexStart;

        [FieldOffset(172)]
        public int RequestId;

        public MarketOverviewResult Spawn(byte[] data, int offset)
        {
            const int itemSize = 8;
            const int itemCount = 20;
            var items = new List<MarketOverviewItem>(itemCount);
            for (var i = 0; i < itemCount; i++)
            {
                fixed (byte* p = &data[offset + 0 + i * itemSize])
                {
                    var item = (*(NetworkMarketResultItem*) p).Spawn(data, offset);
                    if(item.ItemId != 0)
                        items.Add(item);
                }
            }

            return new MarketOverviewResult
            {
                ResultItems = items,
                ItemIndexEnd = this.ItemIndexEnd,
                Padding = this.Padding,
                ItemIndexStart = this.ItemIndexStart,
                RequestId = this.RequestId,
            };
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct NetworkMarketResultItem : INetworkMessageBase<MarketOverviewItem>
    {
        [FieldOffset(0)]
        public int ItemId;

        [FieldOffset(4)]
        public short OpenListing;

        [FieldOffset(6)]
        public short Demand;

        public MarketOverviewItem Spawn(byte[] data, int offset)
        {
            return new MarketOverviewItem
            {
                ItemId = this.ItemId,
                OpenListing = this.OpenListing,
                Demand = this.Demand,
            };
        }
    }
}
