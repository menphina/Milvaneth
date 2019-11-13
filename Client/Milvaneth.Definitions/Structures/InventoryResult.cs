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

using MessagePack;
using System.Collections.Generic;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class InventoryResult : IResult
    {
        public InventoryResult(List<InventoryContainer> containers, long context)
        {
            InventoryContainers = containers;
            Context = context;
        }

        [Key(0)]
        public List<InventoryContainer> InventoryContainers { get; }

        [Key(1)]
        public long Context;
    }
}
