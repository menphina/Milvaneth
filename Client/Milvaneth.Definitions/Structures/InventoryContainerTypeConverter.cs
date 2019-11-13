namespace Milvaneth.Common
{
    public class InventoryContainerTypeConverter
    {
        public static InventoryContainerOffset ToOffset(int containerId)
        {
            InventoryContainerOffset retVal;
            switch (containerId)
            {
                case 0: retVal = (InventoryContainerOffset)0x0; break;
                case 1: retVal = (InventoryContainerOffset)0x1; break;
                case 2: retVal = (InventoryContainerOffset)0x2; break;
                case 3: retVal = (InventoryContainerOffset)0x3; break;
                case 1000: retVal = (InventoryContainerOffset)0x4; break;
                case 2000: retVal = (InventoryContainerOffset)0x5; break;
                case 2001: retVal = (InventoryContainerOffset)0x6; break;
                case 2002: retVal = (InventoryContainerOffset)0x7; break;
                case 2003: retVal = (InventoryContainerOffset)0x8; break;
                case 2004: retVal = (InventoryContainerOffset)0x9; break;
                case 2005: retVal = (InventoryContainerOffset)0xA; break;
                case 2006: retVal = (InventoryContainerOffset)0xB; break;
                case 2007: retVal = (InventoryContainerOffset)0xC; break;
                case 2008: retVal = (InventoryContainerOffset)0xD; break;
                case 2009: retVal = (InventoryContainerOffset)0xE; break;
                case 2010: retVal = (InventoryContainerOffset)0xF; break;
                case 2011: retVal = (InventoryContainerOffset)0x10; break;
                case 2012: retVal = (InventoryContainerOffset)0x11; break;
                case 10000: retVal = (InventoryContainerOffset)0x13; break;
                case 10001: retVal = (InventoryContainerOffset)0x14; break;
                case 10002: retVal = (InventoryContainerOffset)0x15; break;
                case 10003: retVal = (InventoryContainerOffset)0x16; break;
                case 10004: retVal = (InventoryContainerOffset)0x17; break;
                case 10005: retVal = (InventoryContainerOffset)0x18; break;
                case 10006: retVal = (InventoryContainerOffset)0x19; break;
                case 11000: retVal = (InventoryContainerOffset)0x1a; break;
                case 12000: retVal = (InventoryContainerOffset)0x1b; break;
                case 12001: retVal = (InventoryContainerOffset)0x1c; break;
                case 12002: retVal = (InventoryContainerOffset)0x1d; break;
                case 3500: retVal = (InventoryContainerOffset)0x1e; break;
                case 3200: retVal = (InventoryContainerOffset)0x1f; break;
                case 3201: retVal = (InventoryContainerOffset)0x20; break;
                case 3202: retVal = (InventoryContainerOffset)0x21; break;
                case 3203: retVal = (InventoryContainerOffset)0x22; break;
                case 3204: retVal = (InventoryContainerOffset)0x23; break;
                case 3205: retVal = (InventoryContainerOffset)0x24; break;
                case 3206: retVal = (InventoryContainerOffset)0x25; break;
                case 3207: retVal = (InventoryContainerOffset)0x26; break;
                case 3208: retVal = (InventoryContainerOffset)0x27; break;
                case 3209: retVal = (InventoryContainerOffset)0x28; break;
                case 3300: retVal = (InventoryContainerOffset)0x29; break;
                case 3400: retVal = (InventoryContainerOffset)0x2a; break;
                case 20000: retVal = (InventoryContainerOffset)0x2b; break;
                case 20001: retVal = (InventoryContainerOffset)0x2c; break;
                case 20002: retVal = (InventoryContainerOffset)0x2d; break;
                case 20003: retVal = (InventoryContainerOffset)0x2e; break;
                case 20004: retVal = (InventoryContainerOffset)0x2f; break;
                case 22000: retVal = (InventoryContainerOffset)0x30; break;
                case 22001: retVal = (InventoryContainerOffset)0x31; break;
                case 25000: retVal = (InventoryContainerOffset)0x32; break;
                case 25001: retVal = (InventoryContainerOffset)0x33; break;
                case 27000: retVal = (InventoryContainerOffset)0x3d; break;
                case 4000: retVal = (InventoryContainerOffset)0x46; break;
                case 4001: retVal = (InventoryContainerOffset)0x47; break;
                case 4100: retVal = (InventoryContainerOffset)0x48; break;
                case 4101: retVal = (InventoryContainerOffset)0x49; break;

                default: retVal = InventoryContainerOffset.UNKETC; break;
            }

            return retVal;
        }

        public static InventoryContainerId ToId(int containerOffset)
        {
            InventoryContainerId retVal;
            switch (containerOffset)
            {
                case 0x0: retVal = (InventoryContainerId)0; break;
                case 0x1: retVal = (InventoryContainerId)1; break;
                case 0x2: retVal = (InventoryContainerId)2; break;
                case 0x3: retVal = (InventoryContainerId)3; break;
                case 0x4: retVal = (InventoryContainerId)1000; break;
                case 0x5: retVal = (InventoryContainerId)2000; break;
                case 0x6: retVal = (InventoryContainerId)2001; break;
                case 0x7: retVal = (InventoryContainerId)2002; break;
                case 0x8: retVal = (InventoryContainerId)2003; break;
                case 0x9: retVal = (InventoryContainerId)2004; break;
                case 0xA: retVal = (InventoryContainerId)2005; break;
                case 0xB: retVal = (InventoryContainerId)2006; break;
                case 0xC: retVal = (InventoryContainerId)2007; break;
                case 0xD: retVal = (InventoryContainerId)2008; break;
                case 0xE: retVal = (InventoryContainerId)2009; break;
                case 0xF: retVal = (InventoryContainerId)2010; break;
                case 0x10: retVal = (InventoryContainerId)2011; break;
                case 0x11: retVal = (InventoryContainerId)2012; break;
                case 0x13: retVal = (InventoryContainerId)10000; break;
                case 0x14: retVal = (InventoryContainerId)10001; break;
                case 0x15: retVal = (InventoryContainerId)10002; break;
                case 0x16: retVal = (InventoryContainerId)10003; break;
                case 0x17: retVal = (InventoryContainerId)10004; break;
                case 0x18: retVal = (InventoryContainerId)10005; break;
                case 0x19: retVal = (InventoryContainerId)10006; break;
                case 0x1a: retVal = (InventoryContainerId)11000; break;
                case 0x1b: retVal = (InventoryContainerId)12000; break;
                case 0x1c: retVal = (InventoryContainerId)12001; break;
                case 0x1d: retVal = (InventoryContainerId)12002; break;
                case 0x1e: retVal = (InventoryContainerId)3500; break;
                case 0x1f: retVal = (InventoryContainerId)3200; break;
                case 0x20: retVal = (InventoryContainerId)3201; break;
                case 0x21: retVal = (InventoryContainerId)3202; break;
                case 0x22: retVal = (InventoryContainerId)3203; break;
                case 0x23: retVal = (InventoryContainerId)3204; break;
                case 0x24: retVal = (InventoryContainerId)3205; break;
                case 0x25: retVal = (InventoryContainerId)3206; break;
                case 0x26: retVal = (InventoryContainerId)3207; break;
                case 0x27: retVal = (InventoryContainerId)3208; break;
                case 0x28: retVal = (InventoryContainerId)3209; break;
                case 0x29: retVal = (InventoryContainerId)3300; break;
                case 0x2a: retVal = (InventoryContainerId)3400; break;
                case 0x2b: retVal = (InventoryContainerId)20000; break;
                case 0x2c: retVal = (InventoryContainerId)20001; break;
                case 0x2d: retVal = (InventoryContainerId)20002; break;
                case 0x2e: retVal = (InventoryContainerId)20003; break;
                case 0x2f: retVal = (InventoryContainerId)20004; break;
                case 0x30: retVal = (InventoryContainerId)22000; break;
                case 0x31: retVal = (InventoryContainerId)22001; break;
                case 0x32: retVal = (InventoryContainerId)25000; break;
                case 0x33: retVal = (InventoryContainerId)25001; break;
                case 0x3d: retVal = (InventoryContainerId)27000; break;
                case 0x46: retVal = (InventoryContainerId)4000; break;
                case 0x47: retVal = (InventoryContainerId)4001; break;
                case 0x48: retVal = (InventoryContainerId)4100; break;
                case 0x49: retVal = (InventoryContainerId)4101; break;

                default: retVal = InventoryContainerId.UNKETC; break;
            }

            return retVal;
        }
    }
}
