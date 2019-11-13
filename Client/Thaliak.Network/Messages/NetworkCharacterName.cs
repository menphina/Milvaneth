using Milvaneth.Common;
using System.Runtime.InteropServices;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkCharacterName : NetworkMessageProcessor, IResult
    {
        public long CharacterId;
        public string Name;

        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkCharacterName);
        }

        public new static unsafe NetworkCharacterName Consume(byte[] data, int offset)
        {
            fixed (byte* raw = &data[offset])
            {
                return (*(NetworkCharacterNameRaw*) raw).Spawn(data, offset);
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NetworkCharacterNameRaw : INetworkMessageBase<NetworkCharacterName>
    {
        [FieldOffset(0)]
        public long CharacterId;

        public NetworkCharacterName Spawn(byte[] data, int offset)
        {
            return new NetworkCharacterName
            {
                CharacterId = this.CharacterId,
                Name = Helper.ToUtf8String(data, offset,8, 32),
            };
        }
    }
}
