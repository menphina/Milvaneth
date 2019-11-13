// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InventoryResult.cs" company="SyndicatedLife">
//   Copyright(c) 2018 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   InventoryResult.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// Copyright (C) 2019 Menphina Project. All rights reserved.


using Milvaneth.Common;
using System;
using System.Collections.Generic;
using Thaliak.Signatures;

namespace Thaliak.Readers
{
    internal partial class DataReader
    {
        private int _szItemInfo;
        private IntPtr _inventoryPointerMap;
        internal InventoryResult GetInventory()
        {
            var result = new InventoryResult(new List<InventoryContainer>(), GetPlayerStat().CharacterId);

            _szItemInfo = Signature.PointerLib[PointerType.Inventory].DtStep;
            _inventoryPointerMap = _gs.GetPointer(PointerType.Inventory);

            const int offset = (int) InventoryContainerOffset.LAST_AVAILABLE * 24;
            var arr = _gs.Reader.Read(_inventoryPointerMap, offset + 10);

            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.INVENTORY_1));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.INVENTORY_2));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.INVENTORY_3));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.INVENTORY_4));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.EQUIPPING));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.CURRENCIES));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.CRYSTALS));

            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.HIRE_1));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.HIRE_2));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.HIRE_3));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.HIRE_4));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.HIRE_5));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.HIRE_6));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.HIRE_EQUIP));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.HIRE_CURRENCIES));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.HIRE_CRTSTALS));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.HIRE_LISTING));
            
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.AC_MH));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.AC_OH));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.AC_HEAD));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.AC_BODY));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.AC_HANDS));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.AC_BELT));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.AC_LEGS));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.AC_FEET));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.AC_EARRINGS));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.AC_NECK));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.AC_WRISTS));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.AC_RINGS));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.AC_SOULS));

            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.COMPANY_1));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.COMPANY_2));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.COMPANY_3));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.COMPANY_CURRENCIES));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.COMPANY_CRYSTALS));

            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.CHOCOBO_BAG_1));
            result.InventoryContainers.Add(GetInventoryItems(arr, InventoryContainerOffset.CHOCOBO_BAG_2));

            return result;
        }

        internal InventoryResult GetInventory(InventoryContainerId containerType)
        {
            var type = InventoryContainerTypeConverter.ToOffset((int) containerType);
            var result = new InventoryResult(new List<InventoryContainer>(), -1);

            _szItemInfo = Signature.PointerLib[PointerType.Inventory].DtStep;
            _inventoryPointerMap = _gs.GetPointer(PointerType.Inventory);

            const int offset = (int)InventoryContainerOffset.LAST_AVAILABLE * 24;
            var arr = _gs.Reader.Read(_inventoryPointerMap, offset + 10);

            result.InventoryContainers.Add(GetInventoryItems(arr, type));

            return result;
        }

        private unsafe InventoryContainer GetInventoryItems(byte[] entry, InventoryContainerOffset type)
        {
            var offset = (int)type * 24;

            var cid = (InventoryContainerId)BitConverter.ToInt16(entry, offset + 8);
            var container = new InventoryContainer(type, cid);
            var containerAddress = ConvInt(entry, IntPtr.Size, offset);
            if (containerAddress == 0) return container;
            
            int limit;
            switch (type)
            {
                case InventoryContainerOffset.COMPANY_1:
                case InventoryContainerOffset.COMPANY_2:
                case InventoryContainerOffset.COMPANY_3:
                    limit = _szItemInfo * 50;
                    break;
                case InventoryContainerOffset.CRYSTALS:
                case InventoryContainerOffset.HIRE_CRTSTALS:
                case InventoryContainerOffset.COMPANY_CRYSTALS:
                    limit = _szItemInfo * 18;
                    break;
                case InventoryContainerOffset.CURRENCIES:
                case InventoryContainerOffset.HIRE_CURRENCIES:
                case InventoryContainerOffset.COMPANY_CURRENCIES:
                    limit = _szItemInfo * 11;
                    break;
                case InventoryContainerOffset.HIRE_1:
                case InventoryContainerOffset.HIRE_2:
                case InventoryContainerOffset.HIRE_3:
                case InventoryContainerOffset.HIRE_4:
                case InventoryContainerOffset.HIRE_5:
                    limit = _szItemInfo * 25;
                    break;
                case InventoryContainerOffset.EQUIPPING:
                case InventoryContainerOffset.HIRE_EQUIP:
                    limit = _szItemInfo * 14;
                    break;
                case InventoryContainerOffset.HIRE_LISTING:
                    limit = _szItemInfo * 20;
                    break;
                case InventoryContainerOffset.AC_SOULS:
                    limit = _szItemInfo * 19;
                    break;
                default:
                    limit = _szItemInfo * 35;
                    break;
            }

            var buf = _gs.Reader.Read(new IntPtr(containerAddress), limit);
            for (var i = 0; i < limit; i += _szItemInfo)
            {
                fixed (byte* p = &buf[i])
                {
                    var item = *(MemoryInventoryItem*) p;
                    if (item.ItemId <= 0) continue;
                    var mat1 = (item.Materia1Attr << 4) | item.Materia1Grade;
                    var mat2 = (item.Materia2Attr << 4) | item.Materia2Grade;
                    var mat3 = (item.Materia3Attr << 4) | item.Materia3Grade;
                    var mat4 = (item.Materia4Attr << 4) | item.Materia4Grade;
                    var mat5 = (item.Materia5Attr << 4) | item.Materia5Grade;
                    container.InventoryItems.Add(new InventoryItem(item.Amount, item.Durability, item.GlamourId,
                        item.ItemId, item.IsHq, item.SpiritBond, item.Slot, item.ArtisanId, item.DyeId,
                        (short) mat1, (short) mat2, (short) mat3, (short) mat4, (short) mat5));
                }
            }

            return container;
        }
    }
}
