using System;
using System.Collections.Generic;

namespace Milvaneth.Server.Models
{
    public partial class VersionDownload
    {
        public VersionDownload()
        {
            VersionData = new HashSet<VersionData>();
        }

        public string BundleKey { get; set; }
        public string FileServer { get; set; }
        public string[] Files { get; set; }
        public string Argument { get; set; }
        public bool BinaryUpdate { get; set; }
        public bool DataUpdate { get; set; }
        public bool UpdaterUpdate { get; set; }

        public virtual ICollection<VersionData> VersionData { get; set; }
    }
}
