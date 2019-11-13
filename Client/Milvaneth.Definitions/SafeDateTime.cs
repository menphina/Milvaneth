using MessagePack;
using System;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class SafeDateTime
    {
        [SerializationConstructor]
        public SafeDateTime(long ticks)
        {
            Ticks = ticks;
        }

        [Key(0)]
        public long Ticks { get; set; }

        [IgnoreMember]
        public DateTime LocalTime => DateTime.FromBinary(Ticks).ToLocalTime();

        [IgnoreMember]
        public DateTime UtcTime => DateTime.FromBinary(Ticks);

        public static implicit operator SafeDateTime(DateTime time) => new SafeDateTime(time.ToUniversalTime().ToBinary());
#if Server
        public static implicit operator DateTime(SafeDateTime time) => DateTime.FromBinary(time.Ticks);
#else
        public static implicit operator DateTime(SafeDateTime time) => DateTime.FromBinary(time.Ticks).ToLocalTime();
#endif
    }
}
