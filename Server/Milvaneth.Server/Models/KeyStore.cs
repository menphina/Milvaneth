using System;
using System.Collections.Generic;

namespace Milvaneth.Server.Models
{
    public partial class KeyStore
    {
        public KeyStore()
        {
            ApiLog = new HashSet<ApiLog>();
        }

        public long KeyId { get; set; }
        public byte[] Key { get; set; }
        public long HoldingAccount { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public DateTime LastActive { get; set; }
        public int Usage { get; set; }
        public int ReuseCounter { get; set; }
        public int Quota { get; set; }

        public virtual AccountData HoldingAccountNavigation { get; set; }
        public virtual KeyUsage UsageNavigation { get; set; }
        public virtual ICollection<ApiLog> ApiLog { get; set; }
    }
}
