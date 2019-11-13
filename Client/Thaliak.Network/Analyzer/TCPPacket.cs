using Milvaneth.Common;
using System;
using System.Linq;
using System.Net;

namespace Thaliak.Network.Analyzer
{
    public class TCPPacket
    {
        public int GlobalOffset;
        public ushort SourcePort;
        public ushort DestinationPort;
        public byte DataOffset;
        public TCPFlags Flags;

        public byte[] Payload;

        public bool IsValid;

        public TCPPacket(byte[] buffer, int ignoreLength)
        {
            try
            {
                GlobalOffset = ignoreLength;

                SourcePort = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, ignoreLength + 0));
                DestinationPort = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, ignoreLength + 2));

                var offsetAndFlags = (ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(buffer, ignoreLength + 12));
                DataOffset = (byte)((offsetAndFlags >> 12) * 4);
                Flags = (TCPFlags)(offsetAndFlags & 511); // 0b111111111 = 511

                Payload = buffer.Skip(ignoreLength + DataOffset).ToArray();

                IsValid = true;
            }
            catch
            {
                SourcePort = 0;
                DestinationPort = 0;
                DataOffset = 0;
                Flags = TCPFlags.NONE;

                Payload = null;

                IsValid = false;
                IpcClient.SendSignal(Signal.ClientPacketParseFail, new[] {"unable to tcplize"}, true);
            }
        }
    }
}
