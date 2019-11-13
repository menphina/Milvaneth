using System.Runtime.InteropServices;

namespace Thaliak.Readers
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct MemoryPlayerStat
    {
        private const int ExpOffset = 164;

        // Player name +1
        [FieldOffset(88)] internal long CharacterId;
        [FieldOffset(102)] internal short JobID;
        [FieldOffset(106)] internal short PGL;
        [FieldOffset(108)] internal short GLA;
        [FieldOffset(110)] internal short MRD;
        [FieldOffset(112)] internal short ARC;
        [FieldOffset(114)] internal short LNC;
        [FieldOffset(116)] internal short THM;
        [FieldOffset(118)] internal short CNJ;
        [FieldOffset(120)] internal short CRP;
        [FieldOffset(122)] internal short BSM;
        [FieldOffset(124)] internal short ARM;
        [FieldOffset(126)] internal short GSM;
        [FieldOffset(128)] internal short LTW;
        [FieldOffset(130)] internal short WVR;
        [FieldOffset(132)] internal short ALC;
        [FieldOffset(134)] internal short CUL;
        [FieldOffset(136)] internal short MIN;
        [FieldOffset(138)] internal short BTN;
        [FieldOffset(140)] internal short FSH;
        [FieldOffset(142)] internal short ACN;
        [FieldOffset(144)] internal short ROG;
        [FieldOffset(146)] internal short MCH;
        [FieldOffset(148)] internal short DRK;
        [FieldOffset(150)] internal short AST;
        [FieldOffset(152)] internal short SAM;
        [FieldOffset(154)] internal short RDM;
        [FieldOffset(156)] internal short BLU;
        [FieldOffset(158)] internal short GNB;
        [FieldOffset(160)] internal short DNC;
        [FieldOffset(ExpOffset + 0)] internal int PGL_CurrentEXP;
        [FieldOffset(ExpOffset + 4)] internal int GLA_CurrentEXP;
        [FieldOffset(ExpOffset + 8)] internal int MRD_CurrentEXP;
        [FieldOffset(ExpOffset + 12)] internal int ARC_CurrentEXP;
        [FieldOffset(ExpOffset + 16)] internal int LNC_CurrentEXP;
        [FieldOffset(ExpOffset + 20)] internal int THM_CurrentEXP;
        [FieldOffset(ExpOffset + 24)] internal int CNJ_CurrentEXP;
        [FieldOffset(ExpOffset + 28)] internal int CRP_CurrentEXP;
        [FieldOffset(ExpOffset + 32)] internal int BSM_CurrentEXP;
        [FieldOffset(ExpOffset + 36)] internal int ARM_CurrentEXP;
        [FieldOffset(ExpOffset + 40)] internal int GSM_CurrentEXP;
        [FieldOffset(ExpOffset + 44)] internal int LTW_CurrentEXP;
        [FieldOffset(ExpOffset + 48)] internal int WVR_CurrentEXP;
        [FieldOffset(ExpOffset + 52)] internal int ALC_CurrentEXP;
        [FieldOffset(ExpOffset + 56)] internal int CUL_CurrentEXP;
        [FieldOffset(ExpOffset + 60)] internal int MIN_CurrentEXP;
        [FieldOffset(ExpOffset + 64)] internal int BTN_CurrentEXP;
        [FieldOffset(ExpOffset + 68)] internal int FSH_CurrentEXP;
        [FieldOffset(ExpOffset + 72)] internal int ACN_CurrentEXP;
        [FieldOffset(ExpOffset + 76)] internal int ROG_CurrentEXP;
        [FieldOffset(ExpOffset + 80)] internal int MCH_CurrentEXP;
        [FieldOffset(ExpOffset + 84)] internal int DRK_CurrentEXP;
        [FieldOffset(ExpOffset + 88)] internal int AST_CurrentEXP;
        [FieldOffset(ExpOffset + 92)] internal int SAM_CurrentEXP;
        [FieldOffset(ExpOffset + 96)] internal int RDM_CurrentEXP;
        [FieldOffset(ExpOffset + 100)] internal int BLU_CurrentEXP;
        [FieldOffset(ExpOffset + 104)] internal int GNB_CurrentEXP;
        [FieldOffset(ExpOffset + 108)] internal int DNC_CurrentEXP;
    }
}
