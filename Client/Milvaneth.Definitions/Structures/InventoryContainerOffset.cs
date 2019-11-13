// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InventoryResult.cs" company="SyndicatedLife">
//   Copyright(c) 2018 Ryan Wilson &amp;lt;syndicated.life@gmail.com&amp;gt; (http://syndicated.life/)
//   Licensed under the MIT license. See LICENSE.md in the solution root for full license information.
// </copyright>
// <summary>
//   Inventory.cs Implementation
// </summary>
// --------------------------------------------------------------------------------------------------------------------
// Copyright (C) 2019 Menphina Project. All rights reserved.

namespace Milvaneth.Common
{
    public enum InventoryContainerOffset : byte
    {
        INVENTORY_1 = 0x0,
        INVENTORY_2 = 0x1,
        INVENTORY_3 = 0x2,
        INVENTORY_4 = 0x3,
        EQUIPPING = 0x4,
        CURRENCIES = 0x5,
        CRYSTALS = 0x6,
        UNK2002 = 0x7, // sz = 6
        UNK2003 = 0x8, // sz = 16
        UNK2004 = 0x9, // sz = 140
        UNK2005 = 0xA, // sz = 6
        UNK2006 = 0xB, // sz = 6
        UNK2007 = 0xC, // sz = 140
        UNK2008 = 0xD, // sz = 25
        HIRE_EQUIP_MAYBE = 0xE,
        UNK2010 = 0xF, // sz = 240
        UNK2011 = 0x10, // sz = 9
        UNK2012 = 0x11, // sz = 10

        // here is [0x0]
        HIRE_1 = 0x13,
        HIRE_2 = 0x14,
        HIRE_3 = 0x15,
        HIRE_4 = 0x16,
        UNK10004 = 0x17, // retainer chest dummy
        HIRE_5 = 0x18,
        HIRE_6 = 0x19,
        HIRE_EQUIP = 0x1a,
        HIRE_CURRENCIES = 0x1B,
        HIRE_CRTSTALS = 0x1C,
        HIRE_LISTING = 0x1D,

        AC_MH = 0x1E,
        AC_OH = 0x1F,
        AC_HEAD = 0x20,
        AC_BODY = 0x21,
        AC_HANDS = 0x22,
        AC_BELT = 0x23,
        AC_LEGS = 0x24,
        AC_FEET = 0x25,
        AC_EARRINGS = 0x26,
        AC_NECK = 0x27,
        AC_WRISTS = 0x28,
        AC_RINGS = 0x29,
        AC_SOULS = 0x2A,

        COMPANY_1 = 0x2B,
        COMPANY_2 = 0x2C,
        COMPANY_3 = 0x2D,
        UNK20003 = 0x2E, // chest 4
        UNK20004 = 0x2F, // chest 5
        COMPANY_CURRENCIES = 0x30,
        COMPANY_CRYSTALS = 0x31,
        UNK25000 = 0x32,
        UNK25001 = 0x33,

        UNK27000 = 0x3D,

        CHOCOBO_BAG_1 = 0x46,
        CHOCOBO_BAG_2 = 0x47,
        UNK4100 = 0x48, // choco 3
        UNK4101 = 0x49, // choco 4

        LAST_AVAILABLE = CHOCOBO_BAG_2, // Last accessible offset
        UNKETC = 0xFF, // error handling, don't use
    }
}
