using MessagePack;
using System;

namespace Milvaneth.Common.Communication.Login
{
    [MessagePackObject]
    public class ClientChallenge : IMilvanethData
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public string Username { get; set; }
        [Key(2)]
        public long[] Trace { get; set; }
        [Key(3)]
        public byte[] ProofOfWork { get; set; }
        [Key(4)]
        public long SessionId { get; set; }
    }
}
