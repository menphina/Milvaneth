using Milvaneth.Server.Models;
using Milvaneth.Server.Service;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Milvaneth.Server.Data
{
    public class OverviewRepository
    {
        private ITimeService _time;
        private MilvanethDbContext _context;

        public OverviewRepository(ITimeService time, MilvanethDbContext context)
        {
            _time = time;
            _context = context;
        }

        public IEnumerable<OverviewData> CommitRange(long characterId, IEnumerable<OverviewData> data, bool saveChange)
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

            _context.OverviewData.AddRange(insert);

            if (saveChange)
            {
                _context.SaveChanges();
            }

            return insert;
        }

        public OverviewData Commit(long characterId, OverviewData data, bool saveChange)
        {
            if (data.BucketId == Guid.Empty)
            {
                data.BucketId = Guid.NewGuid();
            }

            if (data.ReporterId == 0)
            {
                data.ReporterId = characterId;
            }

            _context.OverviewData.Add(data);

            if (saveChange)
            {
                _context.SaveChanges();
            }

            return data;
        }
    }
}