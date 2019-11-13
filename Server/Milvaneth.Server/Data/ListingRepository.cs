using Milvaneth.Server.Models;
using Milvaneth.Server.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Milvaneth.Server.Data
{
    public class ListingRepository
    {
        private ITimeService _time;
        private MilvanethDbContext _context;

        public ListingRepository(ITimeService time, MilvanethDbContext context)
        {
            _time = time;
            _context = context;
        }

        public IEnumerable<ListingData> CommitRange(long characterId, IEnumerable<ListingData> data, bool saveChange)
        {
            if (data == null)
                throw new InvalidOperationException();

            var bucket = Guid.NewGuid();

            var insert = data
                .Select(
                    x =>
                    {
                        x.BucketId = bucket;
                        x.ReporterId = characterId;
                        return x;
                    });

            _context.ListingData.AddRange(insert);

            if (saveChange)
            {
                _context.SaveChanges();
            }

            return insert;
        }

        public ListingData Commit(long characterId, ListingData data, bool saveChange)
        {
            if (data.BucketId == Guid.Empty)
            {
                data.BucketId = Guid.NewGuid();
            }

            _context.ListingData.Add(data);

            if (saveChange)
            {
                _context.SaveChanges();
            }

            return data;
        }
    }
}
