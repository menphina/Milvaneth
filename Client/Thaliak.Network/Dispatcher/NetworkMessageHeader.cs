using System.Runtime.InteropServices;

namespace Thaliak.Network
{
    [StructLayout(LayoutKind.Explicit)]
    public struct NetworkMessageHeader
    {
        [FieldOffset(4)]
        public int ActorId;

        [FieldOffset(8)]
        public int UserId;

        [FieldOffset(18)]
        public short OpCode;

        [FieldOffset(24)]
        public int Timestamp;
    }
}
