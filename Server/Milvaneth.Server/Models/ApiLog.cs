using System;
using System.Collections.Generic;

namespace Milvaneth.Server.Models
{
    public partial class ApiLog
    {
        public long RecordId { get; set; }
        public DateTime ReportTime { get; set; }
        public long AccountId { get; set; }
        public long KeyId { get; set; }
        public int Operation { get; set; }
        public string Detail { get; set; }
        public string IpAddress { get; set; }

        public virtual AccountData Account { get; set; }
        public virtual KeyStore Key { get; set; }
    }
}
