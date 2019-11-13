using Milvaneth.Server.Data;

namespace Milvaneth.Server.Service
{
    public interface IRepository
    {
        CharacterRepository Character { get; }
        RetainerRepository Retainer { get; }
        HistoryRepository History { get; }
        ListingRepository Listing { get; }
        OverviewRepository Overview { get; }
    }
}
