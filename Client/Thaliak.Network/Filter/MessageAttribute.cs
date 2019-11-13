using System;

namespace Thaliak.Network.Filter
{
    [Flags]
    public enum MessageAttribute
    {
        PortA = 0x000,
        PortB = 0x001,
        PortC = 0x002,
        PortD = 0x003,
        PortE = 0x004,
        PortF = 0x005,
        PortMask = 0x00F,

        DirectionSend = 0x010,
        DirectionReceive = 0x020,
        DirectionMask = 0x0F0,

        CatalogLobby = 0x100,
        CatalogWorld = 0x200,
        CatalogMask = 0xF00,

        NoMatch = 0x7FFFFFFF,
    }

    public static class MessageAttributeHelper
    {
        public static MessageAttribute ToPort(this int value)
        {
            return (MessageAttribute) value;
        }

        public static MessageAttribute GetPort(this MessageAttribute value)
        {
            return value & MessageAttribute.PortMask;
        }

        public static MessageAttribute GetDirection(this MessageAttribute value)
        {
            return value & MessageAttribute.DirectionMask;
        }

        public static MessageAttribute GetCatalog(this MessageAttribute value)
        {
            return value & MessageAttribute.CatalogMask;
        }
    }
}
