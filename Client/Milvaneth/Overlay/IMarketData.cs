using System;

namespace Milvaneth.Overlay
{
    public interface IMarketData
    {
        DateTime UpdateTime { get; set; }
        int Zone { get; set; }
        int World { get; set; }
    }
}
