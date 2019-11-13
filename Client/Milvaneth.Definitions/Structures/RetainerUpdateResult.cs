using MessagePack;
using System.Collections.Generic;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class RetainerUpdateResult : IResult
    {
        [Key(0)]
        public List<RetainerUpdateItem> UpdateItems;
    }
}
