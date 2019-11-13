// ReSharper disable InconsistentNaming

using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class StatusLevelInfo
    {
        [Key(0)]
        public short PGL { get; set; }
        [Key(1)]
        public short GLA { get; set; }
        [Key(2)]
        public short MRD { get; set; }
        [Key(3)]
        public short ARC { get; set; }
        [Key(4)]
        public short LNC { get; set; }
        [Key(5)]
        public short THM { get; set; }
        [Key(6)]
        public short CNJ { get; set; }
        [Key(7)]
        public short CRP { get; set; }
        [Key(8)]
        public short BSM { get; set; }
        [Key(9)]
        public short ARM { get; set; }
        [Key(10)]
        public short GSM { get; set; }
        [Key(11)]
        public short LTW { get; set; }
        [Key(12)]
        public short WVR { get; set; }
        [Key(13)]
        public short ALC { get; set; }
        [Key(14)]
        public short CUL { get; set; }
        [Key(15)]
        public short MIN { get; set; }
        [Key(16)]
        public short BTN { get; set; }
        [Key(17)]
        public short FSH { get; set; }
        [Key(18)]
        public short ACN { get; set; }
        [Key(19)]
        public short ROG { get; set; }
        [Key(20)]
        public short MCH { get; set; }
        [Key(21)]
        public short DRK { get; set; }
        [Key(22)]
        public short AST { get; set; }
        [Key(23)]
        public short SAM { get; set; }
        [Key(24)]
        public short RDM { get; set; }
        [Key(25)]
        public int PGL_CurrentEXP { get; set; }
        [Key(26)]
        public int GLA_CurrentEXP { get; set; }
        [Key(27)]
        public int MRD_CurrentEXP { get; set; }
        [Key(28)]
        public int ARC_CurrentEXP { get; set; }
        [Key(29)]
        public int LNC_CurrentEXP { get; set; }
        [Key(30)]
        public int THM_CurrentEXP { get; set; }
        [Key(31)]
        public int CNJ_CurrentEXP { get; set; }
        [Key(32)]
        public int CRP_CurrentEXP { get; set; }
        [Key(33)]
        public int BSM_CurrentEXP { get; set; }
        [Key(34)]
        public int ARM_CurrentEXP { get; set; }
        [Key(35)]
        public int GSM_CurrentEXP { get; set; }
        [Key(36)]
        public int LTW_CurrentEXP { get; set; }
        [Key(37)]
        public int WVR_CurrentEXP { get; set; }
        [Key(38)]
        public int ALC_CurrentEXP { get; set; }
        [Key(39)]
        public int CUL_CurrentEXP { get; set; }
        [Key(40)]
        public int MIN_CurrentEXP { get; set; }
        [Key(41)]
        public int BTN_CurrentEXP { get; set; }
        [Key(42)]
        public int FSH_CurrentEXP { get; set; }
        [Key(43)]
        public int ACN_CurrentEXP { get; set; }
        [Key(44)]
        public int ROG_CurrentEXP { get; set; }
        [Key(45)]
        public int MCH_CurrentEXP { get; set; }
        [Key(46)]
        public int DRK_CurrentEXP { get; set; }
        [Key(47)]
        public int AST_CurrentEXP { get; set; }
        [Key(48)]
        public int SAM_CurrentEXP { get; set; }
        [Key(49)]
        public int RDM_CurrentEXP { get; set; }

        [Key(50)]
        public short BLU { get; set; }
        [Key(51)]
        public short GNB { get; set; }
        [Key(52)]
        public short DNC { get; set; }
        [Key(53)]
        public int BLU_CurrentEXP { get; set; }
        [Key(54)]
        public int GNB_CurrentEXP { get; set; }
        [Key(55)]
        public int DNC_CurrentEXP { get; set; }
    }
}
