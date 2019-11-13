using Milvaneth.Common;
using System;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkLobbyService : NetworkMessageProcessor
    {
        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkLobbyService);
        }

        public new static unsafe LobbyServiceResult Consume(byte[] data, int offset)
        {
            return new LobbyServiceResult
            {
                ServiceId = BitConverter.ToUInt32(data, offset + 0x10),
                ServiceProvider = Helper.ToUtf8String(data, offset + 0x1C, 0, 0x44),
            };
        }
    }
}