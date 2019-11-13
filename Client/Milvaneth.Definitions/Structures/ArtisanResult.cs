using MessagePack;
using System.Collections.Generic;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class ArtisanResult : IResult
    {
        public ArtisanResult(List<ArtisanInfo> list)
        {
            ArtisanList = list;
        }

        [Key(0)]
        public List<ArtisanInfo> ArtisanList { get; }
    }
}
