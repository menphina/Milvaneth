using MessagePack;
using System;

namespace Milvaneth.Common.Communication.Auth
{
    [MessagePackObject]
    public class AuthRenew : IMilvanethData
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public string Username { get; set; }
        [Key(2)]
        public byte[] RenewToken { get; set; }
    }
}