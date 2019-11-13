using MessagePack;

namespace Milvaneth.Common
{
    [Union(0, typeof(ArtisanResult))]
    [Union(1, typeof(ChatlogResult))]
    [Union(2, typeof(CurrentWorldResult))]
    [Union(3, typeof(InventoryResult))]
    [Union(4, typeof(LobbyCharacterResult))]
    [Union(5, typeof(LobbyServiceResult))]
    [Union(6, typeof(MarketHistoryResult))]
    [Union(7, typeof(MarketListingResult))]
    [Union(8, typeof(MarketOverviewResult))]
    [Union(9, typeof(MarketRequestResult))]
    [Union(10, typeof(RetainerHistoryResult))]
    [Union(11, typeof(RetainerInfoResult))]
    [Union(12, typeof(RetainerUpdateResult))]
    [Union(13, typeof(StatusResult))]

    public interface IResult
    {
    }
}
