using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessagePack;

namespace Milvaneth.Common.Communication.Recovery
{
    [MessagePackObject]
    public class RecoveryRequest : IMilvanethData
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public string Email { get; set; }
        [Key(2)]
        public byte[] Salt { get; set; }
        [Key(3)]
        public byte[] Verifier { get; set; }
        [Key(4)]
        public int GroupParam { get; set; }
        [Key(5)]
        public byte[] OperationToken { get; set; }
    }
}
