using MessagePack;
using System;

namespace Milvaneth.Common.Communication.Data
{
    [MessagePackObject]
    public class AccountUpdate : IMilvanethData
    {
        [Key(0)]
        public SafeDateTime ReportTime { get; set; }
        [Key(1)]
        public string DisplayName { get; set; }
        [Key(2)]
        public long[] Trace { get; set; }
        [Key(3)]
        public byte[] AdditionalData { get; set; }
    }
}