using Milvaneth.Server.Data;
using Milvaneth.Server.Models;

namespace Milvaneth.Server.Service
{
    public class DataRepository : IRepository
    {
        public CharacterRepository Character { get; }
        public RetainerRepository Retainer { get; }
        public HistoryRepository History { get; }
        public ListingRepository Listing { get; }
        public OverviewRepository Overview { get; }

        public DataRepository(ITimeService time, MilvanethDbContext context)
        {
            Character = new CharacterRepository(time, context);
            Retainer = new RetainerRepository(time, context);
            History = new HistoryRepository(time, context);
            Listing = new ListingRepository(time, context);
            Overview = new OverviewRepository(time, context);
        }
    }
}
