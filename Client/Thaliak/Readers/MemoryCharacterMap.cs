using System.Runtime.InteropServices;

namespace Thaliak.Readers
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct MemoryCharacterMap
    {
        // Name string +48
        [FieldOffset(116)] internal int ID;
        [FieldOffset(120)] internal int NPCID1;
        [FieldOffset(128)] internal int NPCID2;
        [FieldOffset(132)] internal int OwnerID;
        [FieldOffset(140)] internal byte Type;
        [FieldOffset(146)] internal byte Distance;
        [FieldOffset(160)] internal float X;
        [FieldOffset(164)] internal float Z;
        [FieldOffset(168)] internal float Y;
        [FieldOffset(176)] internal float Heading;
        [FieldOffset(192)] internal float HitBoxRadius;
        [FieldOffset(396)] internal byte IsGM;
        [FieldOffset(464)] internal byte TargetType;
    }
}
