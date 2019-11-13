using System;
using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class StatusResult : IResult
    {
        [Key(0)]
        public string CharacterName { get; set; }
        [Key(1)]
        public int CharacterHomeWorld { get; set; }
        [Key(2)]
        public int CharacterCurrentWorld { get; set; }
        [Key(3)]
        public long CharacterId { get; set; }
        [Key(4)]
        public SafeDateTime ServerTime { get; set; }
        [Key(5)]
        public int SessionTime { get; set; }

        [Key(6)]
        public StatusLevelInfo LevelInfo { get; set; }
        [Key(7)]
        public StatusCharaInfo CharaInfo { get; set; }
    }
}
