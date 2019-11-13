using System;
using System.Collections.Generic;

namespace Milvaneth.Server.Models
{
    public partial class TokenRevocationList
    {
        public long TokenSerial { get; set; }
        public int Reason { get; set; }
        public DateTime RevokeSince { get; set; }

        public virtual TokenIssueList TokenSerialNavigation { get; set; }
    }
}
