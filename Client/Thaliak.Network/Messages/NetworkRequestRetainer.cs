using Milvaneth.Common;
using System;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkRequestRetainer : NetworkMessageProcessor, IResult
    {
        public long RetainerId;

        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkRequestRetainer);
        }

        public new static unsafe NetworkRequestRetainer Consume(byte[] data, int offset)
        {
            if (BitConverter.ToInt16(data, offset + 6) != 0x0207)
                return null;

            var int1 = BitConverter.ToInt32(data, offset + 8);
            var int2 = BitConverter.ToInt32(data, offset + 12);
            return new NetworkRequestRetainer
            {
                RetainerId = IntToLong(int1, int2)
            };
        }

        private static long IntToLong(int a1, int a2)
        {
            // this is a 'swapped' version
            long b = a1;
            b = b << 32;
            b = b | (uint)a2;
            return b;
        }
    }
}