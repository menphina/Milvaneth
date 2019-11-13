using System;
using System.Collections.Generic;

namespace Milvaneth.Server.Models
{
    public partial class AccountLog
    {
        public long RecordId { get; set; }
        public DateTime ReportTime { get; set; }
        public long AccountId { get; set; }
        public int Message { get; set; }
        public string Detail { get; set; }
        public string IpAddress { get; set; }

        public virtual AccountData Account { get; set; }
    }
}
