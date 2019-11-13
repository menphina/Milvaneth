using MessagePack;
using System;

namespace Milvaneth.Common.Communication.Register
{
    [MessagePackObject]
    public class RegisterForm : IMilvanethData
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public LobbyServiceResult Service { get; set; }
        [Key(2)]
        public LobbyCharacterResult Character { get; set; }
        [Key(3)]
        public string Username { get; set; }
        [Key(4)]
        public string DisplayName { get; set; }
        [Key(5)]
        public string Email { get; set; }
        [Key(6)]
        public long[] Trace { get; set; }
        [Key(7)]
        public byte[] Salt { get; set; }
        [Key(8)]
        public byte[] Verifier { get; set; }
        [Key(9)]
        public int GroupParam { get; set; }
        [Key(10)]
        public byte[] ProofOfWork { get; set; }
        [Key(11)]
        public long SessionId { get; set; }
    }
}
