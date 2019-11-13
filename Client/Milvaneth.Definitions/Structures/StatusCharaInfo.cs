using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class StatusCharaInfo
    {
        [Key(0)]
        public int Id { get; set; }
        [Key(1)]
        public byte IsGm { get; set; }
        [Key(2)]
        public byte Title { get; set; }
        [Key(3)]
        public byte Job { get; set; }
        [Key(4)]
        public byte Level { get; set; }
        [Key(5)]
        public byte Icon { get; set; }
        [Key(6)]
        public byte Status { get; set; }
        [Key(7)]
        public int GilHold { get; set; }
    }
}
