using MessagePack;
using System;
using System.Security.Cryptography;

namespace Milvaneth.Common.Communication.Recovery
{
    [MessagePackObject]
    public class RecoveryGame : IMilvanethData
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
        public long[] Trace { get; set; }
        [Key(5)]
        public byte[] ProofOfWork { get; set; }
    }
}
