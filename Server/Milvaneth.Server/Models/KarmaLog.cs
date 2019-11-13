using System;
using System.Collections.Generic;

namespace Milvaneth.Server.Models
{
    public partial class KarmaLog
    {
        public long RecordId { get; set; }
        public DateTime ReportTime { get; set; }
        public long AccountId { get; set; }
        public int Reason { get; set; }
        public long Before { get; set; }
        public long After { get; set; }

        public virtual AccountData Account { get; set; }
    }
}
