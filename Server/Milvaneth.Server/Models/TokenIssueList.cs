using System;
using System.Collections.Generic;

namespace Milvaneth.Server.Models
{
    public partial class TokenIssueList
    {
        public long TokenSerial { get; set; }
        public long HoldingAccount { get; set; }
        public int Reason { get; set; }
        public DateTime IssueTime { get; set; }
        public DateTime ValidUntil { get; set; }

        public virtual AccountData HoldingAccountNavigation { get; set; }
        public virtual TokenRevocationList TokenRevocationList { get; set; }
    }
}
