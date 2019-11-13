using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class LobbyCharacterItem
    {
        [Key(0)]
        public long UnknownId;

        [Key(1)]
        public long CharacterId;

        [Key(2)]
        public short CurrentWorldId;

        [Key(3)]
        public short HomeWorldId;

        [Key(4)]
        public string CharacterName;

        [Key(5)]
        public string CurrentWorldName;

        [Key(6)]
        public string HomeWorldName;

        [Key(7)]
        public string DetailJson;
    }
}