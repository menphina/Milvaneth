// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InventoryResult.cs" company="SyndicatedLife">
//   Copyright(c) 2018 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   InventoryContainer.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// Copyright (C) 2019 Menphina Project. All rights reserved.

using System.Collections.Generic;
using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class InventoryContainer
    {
        public InventoryContainer(InventoryContainerOffset ict, InventoryContainerId cid) : this(new List<InventoryItem>(), ict, cid)
        {
        }

        [SerializationConstructor]
        public InventoryContainer(List<InventoryItem> items, InventoryContainerOffset ict, InventoryContainerId cid)
        {
            InventoryItems = items;
            ContainerOffset = ict;
            ContainerId = cid;
        }

        [Key(0)]
        public List<InventoryItem> InventoryItems { get; }
        [Key(1)]
        public InventoryContainerOffset ContainerOffset { get; }
        [Key(2)]
        public InventoryContainerId ContainerId { get; }
    }
}
