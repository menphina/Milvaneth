using System;
using System.Collections.Generic;

namespace Milvaneth.Server.Models
{
    public partial class EmailVerifyCode
    {
        public long EventId { get; set; }
        public long AccountId { get; set; }
        public string Email { get; set; }
        public short FailedRetry { get; set; }
        public DateTime ValidTo { get; set; }
        public string Code { get; set; }
        public DateTime SendTime { get; set; }

        public virtual AccountData Account { get; set; }
    }
}
