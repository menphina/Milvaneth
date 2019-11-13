using Milvaneth.Server.Models;
using Milvaneth.Server.Service;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Milvaneth.Server.Data
{
    public class HistoryRepository
    {
        private ITimeService _time;
        private MilvanethDbContext _context;

        public HistoryRepository(ITimeService time, MilvanethDbContext context)
        {
            _time = time;
            _context = context;
        }

        public IEnumerable<HistoryData> CommitRange(long characterId, IEnumerable<HistoryData> data, bool saveChange)
        {
            if (data == null)
                throw new InvalidOperationException();

            var snapshot = data.ToImmutableList();
            var search = snapshot.Select(x => new {x.World, x.PurchaseTime});
            var source = _context.HistoryData
                .Where(x => search.Contains(new {x.World, x.PurchaseTime}))
                .Select(x => new { x.World, x.PurchaseTime });
            var bucket = Guid.NewGuid();

            var insert = snapshot
                .Where(x => source.All(y => y != new {x.World, x.PurchaseTime}))
                .Select(
                    x =>
                    {
                        x.BucketId = bucket;
                        x.ReporterId = characterId;
                        return x;
                    });

            _context.HistoryData.AddRange(insert);

            if (saveChange)
            {
                _context.SaveChanges();
            }

            return insert;
        }

        public HistoryData Commit(long characterId, HistoryData data, bool saveChange)
        {
            var source = _context.HistoryData.Where(x => x.World == data.World && x.PurchaseTime == data.PurchaseTime && x.UnitPrice == data.UnitPrice && x.Quantity == data.Quantity);

            if (source.Any(x => x.BuyerName == data.BuyerName ))
            {
                return data;
            }

            if (data.BucketId == Guid.Empty)
            {
                data.BucketId = Guid.NewGuid();
            }

            data.ReporterId = characterId;

            _context.HistoryData.Add(data);

            if (saveChange)
            {
                _context.SaveChanges();
            }

            return data;
        }
    }
}
