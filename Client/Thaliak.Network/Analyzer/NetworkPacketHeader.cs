using System.Runtime.InteropServices;

namespace Thaliak.Network
{
    [StructLayout(LayoutKind.Explicit)]
    public struct NetworkPacketHeader
    {
        [FieldOffset(16)]
        public long Timestamp;

        [FieldOffset(24)]
        public ushort Length;

        [FieldOffset(30)]
        public ushort Count;

        [FieldOffset(32)]
        public ushort Flags;

        public bool IsCompressed => (Flags & 0xFF00) == 0x0100;

        public bool Malformed => (Flags & 0xFEFE) != 0;
    }
}
