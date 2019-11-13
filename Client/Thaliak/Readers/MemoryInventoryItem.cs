using System.Runtime.InteropServices;

namespace Thaliak.Readers
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct MemoryInventoryItem
    {
        [FieldOffset(0)] internal byte ContainerId;
        [FieldOffset(4)] internal byte Slot;
        [FieldOffset(8)] internal int ItemId;
        [FieldOffset(12)] internal int Amount;
        [FieldOffset(16)] internal short SpiritBond;
        [FieldOffset(18)] internal short Durability;
        [FieldOffset(20)] internal byte IsHq;
        [FieldOffset(0x18)] internal long ArtisanId;
        [FieldOffset(0x20)] internal short Materia1Attr;
        [FieldOffset(0x22)] internal short Materia2Attr;
        [FieldOffset(0x24)] internal short Materia3Attr;
        [FieldOffset(0x26)] internal short Materia4Attr;
        [FieldOffset(0x28)] internal short Materia5Attr;
        [FieldOffset(0x2A)] internal byte Materia1Grade;
        [FieldOffset(0x2B)] internal byte Materia2Grade;
        [FieldOffset(0x2C)] internal byte Materia3Grade;
        [FieldOffset(0x2D)] internal byte Materia4Grade;
        [FieldOffset(0x2E)] internal byte Materia5Grade;
        [FieldOffset(0x2F)] internal byte DyeId;
        [FieldOffset(48)] internal int GlamourId;
    }
}
