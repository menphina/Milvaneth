using MessagePack;
using System;

namespace Milvaneth.Common.Communication.Data
{
    [MessagePackObject]
    public class AccountStatus : IMilvanethData, IMilvanethResponse
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public string Username { get; set; }
        [Key(2)]
        public string DisplayName { get; set; }
        [Key(3)]
        public string Email { get; set; }
        [Key(4)]
        public int EstiKarma { get; set; }
        [Key(5)]
        public int Message { get; set; }
    }
}
