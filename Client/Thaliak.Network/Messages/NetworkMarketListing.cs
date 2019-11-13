using Milvaneth.Common;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkMarketListing : NetworkMessageProcessor
    {
        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkMarketListing);
        }

        public new static unsafe MarketListingResult Consume(byte[] data, int offset)
        {
            fixed (byte* raw = &data[offset])
            {
                return (*(NetworkMarketListingRaw*) raw).Spawn(data, offset);
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NetworkMarketListingRaw : INetworkMessageBase<MarketListingResult>
    {
        [FieldOffset(1520)]
        public byte ListingIndexEnd;

        [FieldOffset(1521)]
        public byte ListingIndexStart;

        [FieldOffset(1522)]
        public short RequestId;

        [FieldOffset(1524)]
        public short Padding;

        public MarketListingResult Spawn(byte[] data, int offset)
        {
            const int itemSize = 152;
            const int itemCount = 10;
            var items = new List<MarketListingItem>(itemCount);
            for (var i = 0; i < itemCount; i++)
            {
                fixed (byte* p = &data[offset + 0 + i * itemSize])
                {
                    var item = (*(NetworkMarketListingItemRaw*) p).Spawn(data, offset + 0 + i * itemSize);
                    if(item.ItemId != 0)
                        items.Add(item);
                }
            }

            return new MarketListingResult
            {
                ListingItems = items,
                ListingIndexEnd = this.ListingIndexEnd,
                ListingIndexStart = this.ListingIndexStart,
                RequestId = this.RequestId,
                Padding = this.Padding,
            };
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NetworkMarketListingItemRaw : INetworkMessageBase<MarketListingItem>
    {
        [FieldOffset(0)]
        public long ListingId;

        [FieldOffset(8)]
        public long RetainerId;

        [FieldOffset(16)]
        public long OwnerId;

        [FieldOffset(24)]
        public long ArtisanId;

        [FieldOffset(32)]
        public int UnitPrice;

        [FieldOffset(36)]
        public int TotalTax;

        [FieldOffset(40)]
        public int Quantity;

        [FieldOffset(44)]
        public int ItemId;

        [FieldOffset(48)]
        public int UpdateTime;

        [FieldOffset(52)]
        public short ContainerId;

        [FieldOffset(54)]
        public short SlotId;

        [FieldOffset(56)]
        public short Condition;

        [FieldOffset(58)]
        public short SpiritBond;

        [FieldOffset(60)]
        public short Materia1;

        [FieldOffset(62)]
        public short Materia2;

        [FieldOffset(64)]
        public short Materia3;

        [FieldOffset(66)]
        public short Materia4;

        [FieldOffset(68)]
        public short Materia5;

        [FieldOffset(70)]
        public short Unknown1;

        [FieldOffset(72)]
        public int Unknown2;

        [FieldOffset(140)]
        public byte IsHq;

        [FieldOffset(141)]
        public byte MateriaCount;

        [FieldOffset(142)]
        public byte OnMannequin;

        [FieldOffset(143)]
        public byte RetainerLocation;

        [FieldOffset(144)]
        public short DyeId;

        [FieldOffset(146)]
        public short Unknown3;

        [FieldOffset(148)]
        public int Unknown4;

        public MarketListingItem Spawn(byte[] data, int offset)
        {
            fixed (byte* p = &data[offset])
            {
                return new MarketListingItem
                {
                    ListingId = this.ListingId,
                    RetainerId = this.RetainerId,
                    OwnerId = this.OwnerId,
                    ArtisanId = this.ArtisanId,
                    UnitPrice = this.UnitPrice,
                    TotalTax = this.TotalTax,
                    Quantity = this.Quantity,
                    ItemId = this.ItemId,
                    UpdateTime = this.UpdateTime,
                    ContainerId = this.ContainerId,
                    SlotId = this.SlotId,
                    Condition = this.Condition,
                    SpiritBond = this.SpiritBond,
                    Materia1 = this.Materia1,
                    Materia2 = this.Materia2,
                    Materia3 = this.Materia3,
                    Materia4 = this.Materia4,
                    Materia5 = this.Materia5,
                    Unknown1 = this.Unknown1,
                    Unknown2 = this.Unknown2,
                    RetainerName = Helper.ToUtf8String(data, offset, 76, 32),
                    PlayerName = Helper.ToUtf8String(data, offset, 108, 32),
                    IsHq = this.IsHq,
                    MateriaCount = this.MateriaCount,
                    OnMannequin = this.OnMannequin,
                    RetainerLocation = this.RetainerLocation,
                    DyeId = this.DyeId,
                    Unknown3 = this.Unknown3,
                    Unknown4 = this.Unknown4,
                };
            }
        }
    }
}
