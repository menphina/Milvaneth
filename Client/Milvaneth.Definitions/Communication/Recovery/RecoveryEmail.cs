using MessagePack;
using System;

namespace Milvaneth.Common.Communication.Recovery
{
    [MessagePackObject]
    public class RecoveryEmail : IMilvanethData
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public string Username { get; set; }
        [Key(2)]
        public string Email { get; set; }
        [Key(3)]
        public string Code { get; set; }
        [Key(4)]
        public long[] Trace { get; set; }
    }
}
