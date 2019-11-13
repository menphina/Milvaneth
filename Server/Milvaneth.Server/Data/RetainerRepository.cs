using Microsoft.EntityFrameworkCore;
using Milvaneth.Server.Models;
using Milvaneth.Server.Service;
using Milvaneth.Server.Statics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Milvaneth.Server.Data
{
    public class RetainerRepository
    {
        private ITimeService _time;
        private MilvanethDbContext _context;

        public RetainerRepository(ITimeService time, MilvanethDbContext context)
        {
            _time = time;
            _context = context;
        }

        public IEnumerable<RetainerData> CommitRange(long accountId, IEnumerable<RetainerData> data, bool saveChange)
        {
            if (data == null)
                throw new InvalidOperationException();

            var snapshot = data.ToImmutableList();

            if (snapshot.Any(x => x.RetainerId <= 0))
                throw new InvalidOperationException();

            var count = snapshot.Count;

            if (count <= 0)
                return null;

            if (count == 1)
                return new[] {Commit(accountId, data.Single(), saveChange)};

            if (count >= 2)
            {
                var search = snapshot.Select(x => x.RetainerId);
                var source = _context.RetainerData.Where(x => search.Contains(x.RetainerId));
                var sourceMirror = source.AsNoTracking().ToImmutableList();
                var remain = snapshot.Where(x => sourceMirror.All(y => y.RetainerId != x.RetainerId));
                //var remain = snapshot.Where(x => source.AsNoTracking().All(y => y.RetainerId != x.RetainerId));

                foreach (var entity in source)
                {
                    var delta = snapshot.Single(x => x.RetainerId == entity.RetainerId);
                    if (delta.RetainerName != null && delta.RetainerName != entity.RetainerName)
                    {
                        _context.DataLog.Add(new DataLog
                        {
                            FromValue = entity.RetainerName,
                            ToValue = delta.RetainerName,
                            Operator = accountId,
                            RecordId = entity.RetainerId,
                            TableColumn = GlobalOperation.COLUMN_RETAINER_NAME,
                            ReportTime = _time.UtcNow
                        });

                        entity.RetainerName = delta.RetainerName;
                    }

                    if (delta.Character != 0 && delta.Character != entity.Character)
                    {
                        _context.DataLog.Add(new DataLog
                        {
                            FromValue = entity.Character.ToString(),
                            ToValue = delta.Character.ToString(),
                            Operator = accountId,
                            RecordId = entity.RetainerId,
                            TableColumn = GlobalOperation.COLUMN_RETAINER_CHARA,
                            ReportTime = _time.UtcNow
                        });

                        entity.Character = delta.Character;
                    }

                    if (delta.World != 0 && delta.World != entity.World)
                    {
                        _context.DataLog.Add(new DataLog
                        {
                            FromValue = entity.World.ToString(),
                            ToValue = delta.World.ToString(),
                            Operator = accountId,
                            RecordId = entity.RetainerId,
                            TableColumn = GlobalOperation.COLUMN_RETAINER_WORLD,
                            ReportTime = _time.UtcNow
                        });

                        entity.World = delta.World;
                    }

                    if (delta.Location != 0 && delta.Location != entity.Location)
                    {
                        _context.DataLog.Add(new DataLog
                        {
                            FromValue = entity.Location.ToString(),
                            ToValue = delta.Location.ToString(),
                            Operator = accountId,
                            RecordId = entity.RetainerId,
                            TableColumn = GlobalOperation.COLUMN_RETAINER_LOCATION,
                            ReportTime = _time.UtcNow
                        });

                        entity.Location = delta.Location;
                    }

                    if (delta.Inventory != null)
                    {
                        entity.Inventory = delta.Inventory;
                    }
                }

                if (remain.Any())
                {
                    _context.RetainerData.AddRange(remain);

                    _context.DataLog.Add(new DataLog
                    {
                        FromValue = null,
                        ToValue = "INITIAL COMMIT MULTIPLE",
                        Operator = accountId,
                        RecordId = 0,
                        TableColumn = GlobalOperation.COLUMN_RETAINER_NEW,
                        ReportTime = _time.UtcNow
                    });
                }

                if (saveChange)
                {
                    _context.SaveChanges();
                }

                return source.Concat(remain);
            }

            return null;
        }

        public RetainerData Commit(long accountId, RetainerData data, bool saveChange)
        {
            if (data == null)
                throw new InvalidOperationException();

            if (data.RetainerId <= 0)
                throw new InvalidOperationException();

            var source = _context.RetainerData.SingleOrDefault(x => x.RetainerId == data.RetainerId);
            var update = false;

            if (source != null)
            {
                if (data.RetainerName != null && data.RetainerName != source.RetainerName)
                {
                    _context.DataLog.Add(new DataLog
                    {
                        FromValue = source.RetainerName,
                        ToValue = data.RetainerName,
                        Operator = accountId,
                        RecordId = source.RetainerId,
                        TableColumn = GlobalOperation.COLUMN_RETAINER_NAME,
                        ReportTime = _time.UtcNow
                    });

                    source.RetainerName = data.RetainerName;

                    update = true;
                }

                if (data.Character != 0 && data.Character != source.Character)
                {
                    _context.DataLog.Add(new DataLog
                    {
                        FromValue = source.Character.ToString(),
                        ToValue = data.Character.ToString(),
                        Operator = accountId,
                        RecordId = source.RetainerId,
                        TableColumn = GlobalOperation.COLUMN_RETAINER_CHARA,
                        ReportTime = _time.UtcNow
                    });

                    source.Character = data.Character;

                    update = true;
                }

                if (data.World != 0 && data.World != source.World)
                {
                    _context.DataLog.Add(new DataLog
                    {
                        FromValue = source.World.ToString(),
                        ToValue = data.World.ToString(),
                        Operator = accountId,
                        RecordId = source.RetainerId,
                        TableColumn = GlobalOperation.COLUMN_RETAINER_WORLD,
                        ReportTime = _time.UtcNow
                    });

                    source.World = data.World;

                    update = true;
                }

                if (data.Location != 0 && data.Location != source.Location)
                {
                    _context.DataLog.Add(new DataLog
                    {
                        FromValue = source.Location.ToString(),
                        ToValue = data.Location.ToString(),
                        Operator = accountId,
                        RecordId = source.RetainerId,
                        TableColumn = GlobalOperation.COLUMN_RETAINER_LOCATION,
                        ReportTime = _time.UtcNow
                    });

                    source.Location = data.Location;

                    update = true;
                }

                if (data.Inventory != null)
                {
                    source.Inventory = data.Inventory;

                    update = true;
                }

                _context.RetainerData.Update(source);
            }

            else
            {
                //source = _context.RetainerData.CreateProxy();
                source = new RetainerData();
                {
                    source.RetainerId = data.RetainerId;
                    source.RetainerName = data.RetainerName;
                    source.Character = data.Character;
                    source.Inventory = data.Inventory;
                    source.World = data.World;
                    source.Location = data.Location;
                }

                _context.RetainerData.Add(source);

                _context.SaveChanges();

                _context.DataLog.Add(new DataLog
                {
                    FromValue = null,
                    ToValue = "INITIAL COMMIT",
                    Operator = accountId,
                    RecordId = source.RetainerId,
                    TableColumn = GlobalOperation.COLUMN_RETAINER_NEW,
                    ReportTime = _time.UtcNow
                });

                update = true;
            }

            if (update && saveChange)
            {
                _context.SaveChanges();
            }

            return source;
        }
    }
}
