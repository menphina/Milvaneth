using Milvaneth.Common;
using System;

namespace Milvaneth.Server.Service
{
    public interface ITimeService
    {
        // DateTime Now { get; }
        DateTime UtcNow { get; }
        SafeDateTime SafeNow { get; }
    }
}
