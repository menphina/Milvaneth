using System;
using System.Collections.Generic;

namespace Milvaneth.Server.Models
{
    public partial class VersionData
    {
        public int VersionId { get; set; }
        public int MilVersion { get; set; }
        public int DataVersion { get; set; }
        public int GameVersion { get; set; }
        public int? UpdateTo { get; set; }
        public bool ForceUpdate { get; set; }
        public string BundleKey { get; set; }
        public string CustomMessage { get; set; }

        public virtual VersionDownload BundleKeyNavigation { get; set; }
    }
}
