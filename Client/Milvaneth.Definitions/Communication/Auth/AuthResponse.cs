using MessagePack;
using System;

namespace Milvaneth.Common.Communication.Auth
{
    [MessagePackObject]
    public class AuthResponse : IMilvanethData, IMilvanethResponse
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public int Message { get; set; }
        [Key(2)]
        public string SessionToken { get; set; }
        [Key(3)]
        public byte[] RenewToken { get; set; }
    }
}