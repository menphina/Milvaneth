using System;
using System.Collections.Generic;

namespace Milvaneth.Server.Models
{
    public partial class DataLog
    {
        public long RecordId { get; set; }
        public DateTime ReportTime { get; set; }
        public short TableColumn { get; set; }
        public long Key { get; set; }
        public string FromValue { get; set; }
        public string ToValue { get; set; }
        public long Operator { get; set; }

        public virtual AccountData OperatorNavigation { get; set; }
    }
}
