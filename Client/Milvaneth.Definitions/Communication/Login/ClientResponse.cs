using MessagePack;
using System;

namespace Milvaneth.Common.Communication.Login
{
    [MessagePackObject]
    public class ClientResponse : IMilvanethData
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public byte[] ClientToken { get; set; }
        [Key(2)]
        public byte[] ClientEvidence { get; set; }
        [Key(3)]
        public long SessionId { get; set; }
    }
}
