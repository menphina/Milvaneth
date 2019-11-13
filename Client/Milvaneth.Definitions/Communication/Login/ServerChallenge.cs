using MessagePack;
using System;

namespace Milvaneth.Common.Communication.Login
{
    [MessagePackObject]
    public class ServerChallenge : IMilvanethData, IMilvanethResponse
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public byte[] Salt { get; set; }
        [Key(2)]
        public int GroupParam { get; set; }
        [Key(3)]
        public byte[] ServerToken { get; set; }
        [Key(4)]
        public int Message { get; set; }
        [Key(5)]
        public long SessionId { get; set; }
        [Key(6)]
        public byte[] ProofOfWork { get; set; }
    }
}