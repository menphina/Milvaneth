using Milvaneth.Common;
using System;

namespace Milvaneth.Server.Service
{
    public class CachedTimeService : ITimeService
    {
        //public DateTime Now => DateTime.Now;
        public static DateTime Utc => DateTime.UtcNow;

        public DateTime UtcNow => DateTime.UtcNow;

        public SafeDateTime SafeNow => UtcNow;
    }
}
