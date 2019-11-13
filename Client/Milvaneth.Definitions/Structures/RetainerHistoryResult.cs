using MessagePack;
using System.Collections.Generic;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class RetainerHistoryResult : IResult
    {
        [Key(0)]
        public long RetainerId;
        [Key(1)]
        public List<RetainerHistoryItem> HistoryItems;
    }
}
