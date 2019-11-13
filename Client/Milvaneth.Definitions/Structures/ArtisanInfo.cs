using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class ArtisanInfo
    {
        public ArtisanInfo(long characterId, string characterName, bool fromMemory)
        {
            CharacterId = characterId;
            CharacterName = characterName;
            FromMemory = fromMemory;

            IsValid = !string.IsNullOrEmpty(CharacterName);
        }
        [Key(0)]
        public long CharacterId { get; }
        [Key(1)]
        public string CharacterName { get; }
        [Key(2)]
        public bool FromMemory { get; }
        [IgnoreMember]
        public bool IsValid { get; }
    }
}
