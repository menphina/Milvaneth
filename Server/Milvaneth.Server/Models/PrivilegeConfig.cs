using System;
using System.Collections.Generic;

namespace Milvaneth.Server.Models
{
    public partial class PrivilegeConfig
    {
        public PrivilegeConfig()
        {
            AccountData = new HashSet<AccountData>();
        }

        public int PrivilegeLevel { get; set; }
        public string Name { get; set; }
        public bool AccessData { get; set; }
        public bool Login { get; set; }
        public bool IgnoreKarma { get; set; }
        public bool AccessStatics { get; set; }
        public bool Debug { get; set; }
        public bool BatchRead { get; set; }
        public bool BatchWrite { get; set; }
        public bool AccountOperation { get; set; }
        public bool ReleaseUpdate { get; set; }
        public bool DeleteRecord { get; set; }
        public bool AccountManagement { get; set; }

        public virtual ICollection<AccountData> AccountData { get; set; }
    }
}
