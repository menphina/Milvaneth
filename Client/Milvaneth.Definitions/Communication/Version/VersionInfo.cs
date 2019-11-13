using MessagePack;
using System;

namespace Milvaneth.Common.Communication.Version
{
    [MessagePackObject]
    public class VersionInfo : IMilvanethData, IMilvanethResponse
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public int EligibleMilvanethVersion { get; set; }
        [Key(2)]
        public int EligibleDataVersion { get; set; }
        [Key(3)]
        public int EligibleGameVersion { get; set; }
        [Key(4)]
        public string EligibleBundleKey { get; set; }
        [Key(5)]
        public int Message { get; set; }
        [Key(6)]
        public string DisplayMessage { get; set; }
        [Key(7)]
        public bool ForceUpdate { get; set; }
    }
}
