using MessagePack;
using System;

namespace Milvaneth.Common.Communication.Login
{
    [MessagePackObject]
    public class ServerResponse : IMilvanethData, IMilvanethResponse
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public int Message { get; set; }
        [Key(2)]
        public byte[] AuthToken { get; set; }
    }
}
