using MessagePack;
using System;

namespace Milvaneth.Common.Communication.Data
{
    [MessagePackObject]
    public class OverviewRequest : IMilvanethData
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public int[] QueryItems { get; set; }
    }
}
