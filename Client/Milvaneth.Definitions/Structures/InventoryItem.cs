// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InventoryResult.cs" company="SyndicatedLife">
//   Copyright(c) 2018 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   InventoryItem.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// Copyright (C) 2019 Menphina Project. All rights reserved.

using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class InventoryItem
    {
        [SerializationConstructor]
        public InventoryItem(int amount, int durability, int glamourId, int itemId, 
            byte hqFlag, int spiritBond, int slot, long artisanId, byte dyeId, 
            short materia1, short materia2, short materia3, short materia4, short materia5)
        {
            Amount = amount;
            Durability = durability;
            GlamourId = glamourId;
            ItemId = itemId;
            HqFlag = hqFlag;
            SpiritBond = spiritBond;
            Slot = slot;
            ArtisanId = artisanId;
            DyeId = dyeId;
            Materia1 = materia1;
            Materia2 = materia2;
            Materia3 = materia3;
            Materia4 = materia4;
            Materia5 = materia5;
            IsHq = HqFlag == 1;
            DurabilityPercent = (double) decimal.Divide(this.Durability, 30000);
            SpiritBondPercent = (double) decimal.Divide(this.SpiritBond, 10000);
        }

        [Key(0)]
        public int Amount { get; }
        [Key(1)]
        public int Durability { get; }
        [IgnoreMember]
        public double DurabilityPercent { get; }
        [Key(2)]
        public int GlamourId { get; }
        [Key(3)]
        public int ItemId { get; }
        [Key(4)]
        public byte HqFlag { get; }
        [IgnoreMember]
        public bool IsHq { get; }
        [Key(5)]
        public int SpiritBond { get; }
        [IgnoreMember]
        public double SpiritBondPercent { get; }
        [Key(6)]
        public int Slot { get; }
        [Key(7)]
        public long ArtisanId { get; }
        [Key(8)]
        public byte DyeId { get; }
        [Key(9)]
        public short Materia1 { get; }
        [Key(10)]
        public short Materia2 { get; }
        [Key(11)]
        public short Materia3 { get; }
        [Key(12)]
        public short Materia4 { get; }
        [Key(13)]
        public short Materia5 { get; }
        [Key(14)]
        public int UnitPrice { get; set; }
    }
}
