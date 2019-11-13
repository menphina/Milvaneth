using Milvaneth.Common;
using System.Runtime.InteropServices;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkClientTrigger : NetworkMessageProcessor, IResult
    {
        public short CommandId;
        public byte Unknown1;
        public byte Unknown2;
        public int Param11;
        public int Param12;
        public int Param2;
        public int Param3;
        public int Param4;
        public int Param5;

        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkClientTrigger);
        }

        public new static unsafe NetworkClientTrigger Consume(byte[] data, int offset)
        {
            fixed (byte* raw = &data[offset])
            {
                var dat = *(NetworkClientTriggerRaw*)raw;
                if (dat.CommandId !=
                    MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkTriggerUpdateSelling))
                    return null;

                return new NetworkClientTrigger
                    {
                        CommandId = dat.CommandId,
                        Param11 = dat.Param11,
                        Param12 = dat.Param12,
                    };
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NetworkClientTriggerRaw
    {
        [FieldOffset(0)]
        public short CommandId;

        [FieldOffset(2)]
        public byte Unknown1;

        [FieldOffset(3)]
        public byte Unknown2;

        [FieldOffset(4)]
        public int Param11;

        [FieldOffset(8)]
        public int Param12;

        [FieldOffset(12)]
        public int Param2;

        [FieldOffset(16)]
        public int Param4;

        [FieldOffset(20)]
        public int Param5;

        [FieldOffset(24)]
        public int Param3;
    }
}
