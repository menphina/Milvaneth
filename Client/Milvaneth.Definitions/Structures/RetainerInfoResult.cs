using MessagePack;
using System.Collections.Generic;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class RetainerInfoResult : IResult
    {
        [Key(0)]
        public List<RetainerInfoItem> RetainerInfo { get; }

        public RetainerInfoResult(List<RetainerInfoItem> items)
        {
            RetainerInfo = items;
        }

    }
}
