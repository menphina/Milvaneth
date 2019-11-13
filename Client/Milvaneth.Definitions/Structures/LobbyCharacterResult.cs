using System.Collections.Generic;
using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class LobbyCharacterResult : IResult
    {
        [Key(0)]
        public byte MessageCounter;

        [Key(1)]
        public byte MessageCount;

        [Key(2)]
        public List<LobbyCharacterItem> CharacterItems;
    }
}
