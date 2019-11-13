namespace Thaliak.Signatures
{
    public enum PointerType
    {
        ChatLogEntry = SignatureType.ChatLog | Indexer.Index01,
        MapInfo = SignatureType.MapInfo | Indexer.Index01,
        SessionUpTime = SignatureType.MapInfo | Indexer.Index02,

        ServerTime = SignatureType.ServerTime | Indexer.Index01,

        PlayerStat = SignatureType.PlayerStat | Indexer.Index01,
#if EnableMarketMemory
        ResultListLength = SignatureType.Market | Indexer.Index01,
        TransactionListLength = SignatureType.Market | Indexer.Index02,
        HistoryListLength = SignatureType.Market | Indexer.Index03,
        ResultList = SignatureType.Market | Indexer.Index04,
        TransactionList = SignatureType.Market | Indexer.Index05,
        HistoryList = SignatureType.Market | Indexer.Index06,
        TransactionListEx = SignatureType.Market | Indexer.Index07,
        ResultStrList = SignatureType.Market | Indexer.Index08,
        TransactionStrList = SignatureType.Market | Indexer.Index09,
        HistoryStrList = SignatureType.Market | Indexer.Index0A,
        HistoryListUtd = SignatureType.Market | Indexer.Index0B,
        CurrentItemId = SignatureType.Market | Indexer.Index0C,
        IsSeriesBFreed = SignatureType.Market | Indexer.Index0D,
        TotalWindowsCounter = SignatureType.Market | Indexer.Index0E,
        CurrentTargetStatus = SignatureType.Market | Indexer.Index0F,
#endif
        CharacterMap = SignatureType.Combatant | Indexer.Index01,
        CharacterExtra = SignatureType.Combatant | Indexer.Index02,

        Inventory = SignatureType.Inventory | Indexer.Index01,
        CurrentGil = SignatureType.Inventory | Indexer.Index02,

        ArtisanList = SignatureType.ArtisanList | Indexer.Index01,

        LocalWorldName = SignatureType.ThreadStack | Indexer.Index01,
        NotSupported = SignatureType.Invalid | Indexer.IndexFF,
    }
}
