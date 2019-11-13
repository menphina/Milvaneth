using Milvaneth.Common;
using System.Runtime.InteropServices;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkPlayerSpawn : NetworkMessageProcessor, IResult
    {
        public short CurrentWorldId;
        public short HomeWorldId;

        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkPlayerSpawn);
        }

        public new static unsafe NetworkPlayerSpawn Consume(byte[] data, int offset)
        {
            fixed (byte* raw = &data[offset])
            {
                return (*(NetworkPlayerSpawnRaw*) raw).Spawn(data, offset);
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct NetworkPlayerSpawnRaw : INetworkMessageBase<NetworkPlayerSpawn>
    {
        [FieldOffset(4)]
        public short CurrentWorldId;

        [FieldOffset(6)]
        public short HomeWorldId;

        public NetworkPlayerSpawn Spawn(byte[] data, int offset)
        {
            return new NetworkPlayerSpawn
            {
                CurrentWorldId = this.CurrentWorldId,
                HomeWorldId = this.HomeWorldId,
            };
        }
    }
}
