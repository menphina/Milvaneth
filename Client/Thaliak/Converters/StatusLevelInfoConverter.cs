using Milvaneth.Common;
using Thaliak.Readers;
// ReSharper disable InconsistentNaming

namespace Thaliak.Converters
{
    internal class StatusLevelInfoConverter
    {
        internal static StatusLevelInfo FromStruct(MemoryPlayerStat ps)
        {
            return new StatusLevelInfo()
            {
                PGL = ps.PGL,
                GLA = ps.GLA,
                MRD = ps.MRD,
                ARC = ps.ARC,
                LNC = ps.LNC,
                THM = ps.THM,
                CNJ = ps.CNJ,
                CRP = ps.CRP,
                BSM = ps.BSM,
                ARM = ps.ARM,
                GSM = ps.GSM,
                LTW = ps.LTW,
                WVR = ps.WVR,
                ALC = ps.ALC,
                CUL = ps.CUL,
                MIN = ps.MIN,
                BTN = ps.BTN,
                FSH = ps.FSH,
                ACN = ps.ACN,
                ROG = ps.ROG,
                MCH = ps.MCH,
                DRK = ps.DRK,
                AST = ps.AST,
                SAM = ps.SAM,
                RDM = ps.RDM,
                BLU = ps.BLU,
                GNB = ps.GNB,
                DNC = ps.DNC,
                PGL_CurrentEXP = ps.PGL_CurrentEXP,
                GLA_CurrentEXP = ps.GLA_CurrentEXP,
                MRD_CurrentEXP = ps.MRD_CurrentEXP,
                ARC_CurrentEXP = ps.ARC_CurrentEXP,
                LNC_CurrentEXP = ps.LNC_CurrentEXP,
                THM_CurrentEXP = ps.THM_CurrentEXP,
                CNJ_CurrentEXP = ps.CNJ_CurrentEXP,
                CRP_CurrentEXP = ps.CRP_CurrentEXP,
                BSM_CurrentEXP = ps.BSM_CurrentEXP,
                ARM_CurrentEXP = ps.ARM_CurrentEXP,
                GSM_CurrentEXP = ps.GSM_CurrentEXP,
                LTW_CurrentEXP = ps.LTW_CurrentEXP,
                WVR_CurrentEXP = ps.WVR_CurrentEXP,
                ALC_CurrentEXP = ps.ALC_CurrentEXP,
                CUL_CurrentEXP = ps.CUL_CurrentEXP,
                MIN_CurrentEXP = ps.MIN_CurrentEXP,
                BTN_CurrentEXP = ps.BTN_CurrentEXP,
                FSH_CurrentEXP = ps.FSH_CurrentEXP,
                ACN_CurrentEXP = ps.ACN_CurrentEXP,
                ROG_CurrentEXP = ps.ROG_CurrentEXP,
                MCH_CurrentEXP = ps.MCH_CurrentEXP,
                DRK_CurrentEXP = ps.DRK_CurrentEXP,
                AST_CurrentEXP = ps.AST_CurrentEXP,
                SAM_CurrentEXP = ps.SAM_CurrentEXP,
                RDM_CurrentEXP = ps.RDM_CurrentEXP,
                BLU_CurrentEXP = ps.BLU_CurrentEXP,
                GNB_CurrentEXP = ps.GNB_CurrentEXP,
                DNC_CurrentEXP = ps.DNC_CurrentEXP,
            };
        }
    }
}
