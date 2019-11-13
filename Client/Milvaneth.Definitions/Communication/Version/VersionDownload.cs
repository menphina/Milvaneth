using MessagePack;
using System;

namespace Milvaneth.Common.Communication.Version
{
    [MessagePackObject]
    public class VersionDownload : IMilvanethData, IMilvanethResponse
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public int Message { get; set; }
        [Key(2)]
        public string FileServer { get; set; } // do not need to guarantee host 
        [Key(3)]
        public string[] Files { get; set; } // should be pack files. see updater comment
        [Key(4)]
        public string Argument { get; set; } // "-target <?installdir> -data <?datadir> -source <?tempdir> -method <?method>"
        [Key(5)]
        public bool BinaryUpdate { get; set; }
        [Key(6)]
        public bool DataUpdate { get; set; }
        [Key(7)]
        public bool UpdaterUpdate { get; set; }
    }
}
