using System.Runtime.InteropServices;

namespace Thaliak.Readers
{
    internal class MemoryArtisanEntity
    {
        internal long CharacterId;
        internal string CharacterName;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct MemoryArtisanEntityRaw
    {
        // Name +0
        [FieldOffset(0x40)] internal long CharacterId;
    }
}
