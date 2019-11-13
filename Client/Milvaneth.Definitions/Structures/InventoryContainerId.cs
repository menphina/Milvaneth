namespace Milvaneth.Common
{
    public enum InventoryContainerId : short
    {
        INVENTORY_1 = 0,
        INVENTORY_2 = 1,
        INVENTORY_3 = 2,
        INVENTORY_4 = 3,
        EQUIPPING = 1000,
        CURRENCIES = 2000,
        CRYSTALS = 2001,
        UNK2002 = 2002, // sz = 6
        UNK2003 = 2003, // sz = 16
        UNK2004 = 2004, // sz = 140
        UNK2005 = 2005, // sz = 6
        UNK2006 = 2006, // sz = 6
        UNK2007 = 2007, // sz = 140
        UNK2008 = 2008, // sz = 25
        HIRE_EQUIP_MAYBE = 2009,
        UNK2010 = 2010, // sz = 240
        UNK2011 = 2011, // sz = 9
        UNK2012 = 2012, // sz = 10

        // here is [0x0]
        HIRE_1 = 10000,
        HIRE_2 = 10001,
        HIRE_3 = 10002,
        HIRE_4 = 10003,
        UNK10004 = 10004, // retainer chest dummy
        HIRE_5 = 10005,
        HIRE_6 = 10006,
        HIRE_EQUIP = 11000,
        HIRE_CURRENCIES = 12000,
        HIRE_CRTSTALS = 12001,
        HIRE_LISTING = 12002,

        AC_MH = 3500,
        AC_OH = 3200,
        AC_HEAD = 3201,
        AC_BODY = 3202,
        AC_HANDS = 3203,
        AC_BELT = 3204,
        AC_LEGS = 3205,
        AC_FEET = 3206,
        AC_EARRINGS = 3207,
        AC_NECK = 3208,
        AC_WRISTS = 3209,
        AC_RINGS = 3300,
        AC_SOULS = 3400,

        COMPANY_1 = 20000,
        COMPANY_2 = 20001,
        COMPANY_3 = 20002,
        UNK20003 = 20003, // chest 4
        UNK20004 = 20004, // chest 5
        COMPANY_CURRENCIES = 22000,
        COMPANY_CRYSTALS = 22001,
        UNK25000 = 25000,
        UNK25001 = 25001,

        UNK27000 = 27000,

        CHOCOBO_BAG_1 = 4000,
        CHOCOBO_BAG_2 = 4001,
        UNK4100 = 4100, // choco 3
        UNK4101 = 4101, // choco 4

        DISCARD = -1,
        UNKETC = -2, // error handling, don't use
    }
}
