using System;
using System.Collections.Generic;

namespace Milvaneth.Server.Models
{
    public partial class KeyUsage
    {
        public KeyUsage()
        {
            KeyStore = new HashSet<KeyStore>();
        }

        public int Usage { get; set; }
        public string Name { get; set; }
        public bool ProveIdentity { get; set; }
        public bool CreateSession { get; set; }
        public bool RenewSession { get; set; }
        public bool GetChangeToken { get; set; }
        public bool ChangePassword { get; set; }
        public bool AccessData { get; set; }
        public bool BatchRead { get; set; }
        public bool BatchWrite { get; set; }

        public virtual ICollection<KeyStore> KeyStore { get; set; }
    }
}
