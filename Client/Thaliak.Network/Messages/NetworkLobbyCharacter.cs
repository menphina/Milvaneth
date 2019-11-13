using System.Collections.Generic;
using System.Runtime.InteropServices;
using Milvaneth.Common;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkLobbyCharacter : NetworkMessageProcessor
    {
        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkLobbyCharacter);
        }

        public new static unsafe LobbyCharacterResult Consume(byte[] data, int offset)
        {
            const int itemSize = 0x4A0;
            var itemCount = data[offset + 0x9];
            var items = new List<LobbyCharacterItem>(itemCount);
            for (var i = 0; i < itemCount; i++)
            {
                fixed (byte* p = &data[offset + 0x50 + i * itemSize])
                {
                    var item = (*(NetworkLobbyCharacterInfoRaw*)p).Spawn(data, offset + 0x50 + i * itemSize);
                    if (item.CharacterId != 0)
                        items.Add(item);
                }
            }
            return new LobbyCharacterResult
            {
                MessageCounter = data[offset + 0x8],
                MessageCount = data[offset + 0x9],
                CharacterItems = items,
            };
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NetworkLobbyCharacterInfoRaw : INetworkMessageBase<LobbyCharacterItem>
    {
        [FieldOffset(0x00)]
        public long UnknownId;

        [FieldOffset(0x08)]
        public long CharacterId;

        [FieldOffset(0x18)]
        public short CurrentWorldId;

        [FieldOffset(0x1A)]
        public short HomeWorldId;

        public LobbyCharacterItem Spawn(byte[] data, int offset)
        {
            fixed (byte* p = &data[offset])
            {
                return new LobbyCharacterItem
                {
                    UnknownId = this.UnknownId,
                    CharacterId = this.CharacterId,
                    CurrentWorldId = this.CurrentWorldId,
                    HomeWorldId = this.HomeWorldId,
                    CharacterName = Helper.ToUtf8String(data, offset, 0x25, 0x20),
                    CurrentWorldName = Helper.ToUtf8String(data, offset, 0x45, 0x20),
                    HomeWorldName = Helper.ToUtf8String(data, offset, 0x65, 0x20),
                    DetailJson = Helper.ToUtf8String(data, offset, 0x85, 0x400),
                };
            }
        }
    }
}