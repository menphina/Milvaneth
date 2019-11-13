namespace Thaliak.Signatures
{
    public enum SignatureType
    {
        ChatLog = 0x0100,
        MapInfo = 0x0200,
        ServerTime = 0x0300,
        PlayerStat = 0x0400,
#if EnableMarketMemory
        Market = 0x0500,
#endif
        Inventory = 0x0700,
        //CurrentGil = 0x0900,
        ArtisanList = 0x0A00,
        Combatant = 0x0B00,

        ThreadStack = 0xFE00,
        Invalid = 0xFF00,
    }
}
