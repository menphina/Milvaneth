using System.Runtime.InteropServices;

namespace Thaliak.Readers
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct MemoryCharacterExtra
    {
        [FieldOffset(132)] internal short CurrentWorldId;
        [FieldOffset(134)] internal short HomeWorldId;
        [FieldOffset(144)] internal int HPCurrent;
        [FieldOffset(148)] internal int HPMax;
        [FieldOffset(152)] internal int MPCurrent;
        [FieldOffset(156)] internal int MPMax;
        [FieldOffset(160)] internal short TPCurrent;
        [FieldOffset(162)] internal short GPCurrent;
        [FieldOffset(164)] internal short GPMax;
        [FieldOffset(166)] internal short CPCurrent;
        [FieldOffset(168)] internal short CPMax;
        [FieldOffset(170)] internal byte Title;
        [FieldOffset(200)] internal byte Job;
        [FieldOffset(202)] internal byte Level;
        [FieldOffset(204)] internal byte Icon;
        [FieldOffset(210)] internal byte Status;
        [FieldOffset(217)] internal byte GrandCompany;
        [FieldOffset(217)] internal byte GrandCompanyRank;
        [FieldOffset(221)] internal byte DifficultyRank;
        [FieldOffset(225)] internal byte AgroFlags;
        [FieldOffset(245)] internal byte CombatFlags;

    }
}
