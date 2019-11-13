using MessagePack;
using System;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class PackedResult : IMilvanethData
    {
        [Key(0)]
        public PackedResultType Type { get; }

        [Key(1)]
        public IResult Result { get; }

        [Key(2)]
        public SafeDateTime ReportTime { get; }

        public PackedResult(PackedResultType type, IResult result)
        {
            Type = type;
            Result = result;
            ReportTime = DateTime.UtcNow;
        }

        [SerializationConstructor]
        public PackedResult(PackedResultType type, IResult result, SafeDateTime reportTime)
        {
            Type = type;
            Result = result;
            ReportTime = reportTime;
        }
    }
}
