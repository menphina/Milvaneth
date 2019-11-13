using Milvaneth.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Thaliak.Readers;

namespace Thaliak.Converters
{
    internal class ArtisanResultConverter
    {
        internal static ArtisanResult FromStruct(List<MemoryArtisanEntity> entityList)
        {
            var list = entityList.Select(x => new ArtisanInfo(x.CharacterId, x.CharacterName, true)).ToList();
            return new ArtisanResult(list);
        }
    }
}
