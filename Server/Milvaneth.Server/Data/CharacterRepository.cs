using Milvaneth.Server.Models;
using Milvaneth.Server.Service;
using Milvaneth.Server.Statics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Milvaneth.Server.Data
{
    public class CharacterRepository
    {
        private ITimeService _time;
        private MilvanethDbContext _context;

        public CharacterRepository(ITimeService time, MilvanethDbContext context)
        {
            _time = time;
            _context = context;
        }

        public IEnumerable<CharacterData> CommitRange(long accountId, IEnumerable<CharacterData> data, bool saveChange)
        {
            if(data == null)
                throw new InvalidOperationException();

            var snapshot = data.ToImmutableList();

            if (snapshot.Any(x => x.CharacterId <= 0))
                throw new InvalidOperationException();

            var count = snapshot.Count;

            if (count <= 0)
                return null;

            if (count == 1)
                return new[] {Commit(accountId, snapshot.Single(), saveChange)};

            if (count >= 2)
            {
                var search = snapshot.Select(x => x.CharacterId);
                var source = _context.CharacterData.Where(x => search.Contains(x.CharacterId));
                var sourceMirror = source.AsNoTracking().ToImmutableList();
                var remain = snapshot.Where(x => sourceMirror.All(y => y.CharacterId != x.CharacterId));
                //var remain = snapshot.Where(x => source.AsNoTracking().All(y => y.CharacterId != x.CharacterId));

                foreach (var entity in source)
                {
                    var delta = snapshot.Single(x => x.CharacterId == entity.CharacterId);
                    if (delta.CharacterName != null && delta.CharacterName != entity.CharacterName)
                    {
                        _context.DataLog.Add(new DataLog
                        {
                            FromValue = entity.CharacterName,
                            ToValue = delta.CharacterName,
                            Operator = accountId,
                            RecordId = entity.CharacterId,
                            TableColumn = GlobalOperation.COLUMN_CHARACTER_NAME,
                            ReportTime = _time.UtcNow
                        });

                        entity.CharacterName = delta.CharacterName;
                    }

                    if (delta.AccountId != 0 && delta.AccountId != entity.AccountId)
                    {
                        _context.DataLog.Add(new DataLog
                        {
                            FromValue = entity.AccountId.ToString(),
                            ToValue = delta.AccountId.ToString(),
                            Operator = accountId,
                            RecordId = entity.CharacterId,
                            TableColumn = GlobalOperation.COLUMN_CHARACTER_ACCOUNT,
                            ReportTime = _time.UtcNow
                        });

                        entity.AccountId = delta.AccountId;
                    }

                    if (delta.ServiceId != 0 && delta.ServiceId != entity.ServiceId)
                    {
                        _context.DataLog.Add(new DataLog
                        {
                            FromValue = entity.ServiceId.ToString(),
                            ToValue = delta.ServiceId.ToString(),
                            Operator = accountId,
                            RecordId = entity.CharacterId,
                            TableColumn = GlobalOperation.COLUMN_CHARACTER_SERVICE,
                            ReportTime = _time.UtcNow
                        });

                        entity.ServiceId = delta.ServiceId;
                    }

                    if (delta.HomeWorld != 0 && delta.HomeWorld != entity.HomeWorld)
                    {
                        _context.DataLog.Add(new DataLog
                        {
                            FromValue = entity.HomeWorld.ToString(),
                            ToValue = delta.HomeWorld.ToString(),
                            Operator = accountId,
                            RecordId = entity.CharacterId,
                            TableColumn = GlobalOperation.COLUMN_CHARACTER_WORLD,
                            ReportTime = _time.UtcNow
                        });

                        entity.HomeWorld = delta.HomeWorld;
                    }

                    if (delta.RetainerList != null && !delta.RetainerList.SequenceEqual(entity.RetainerList))
                    {
                        entity.RetainerList = delta.RetainerList;
                    }

                    if (delta.JobLevels != null && !delta.JobLevels.SequenceEqual(entity.JobLevels))
                    {
                        entity.JobLevels = delta.JobLevels;
                    }

                    if (delta.Inventory != null)
                    {
                        entity.Inventory = delta.Inventory;
                    }

                    if (delta.GilHold != null)
                    {
                        entity.GilHold = delta.GilHold;
                    }

                    _context.CharacterData.Update(entity);
                }

                if (remain.Any())
                {
                    _context.CharacterData.AddRange(remain);

                    _context.DataLog.Add(new DataLog
                    {
                        FromValue = null,
                        ToValue = "INITIAL COMMIT MULTIPLE",
                        Operator = accountId,
                        RecordId = 0,
                        TableColumn = GlobalOperation.COLUMN_CHARACTER_NEW,
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

        public CharacterData Commit(long accountId, CharacterData data, bool saveChange)
        {
            if (data == null)
                throw new InvalidOperationException();

            if (data.CharacterId <= 0)
                throw new InvalidOperationException();

            var source = _context.CharacterData.SingleOrDefault(x => x.CharacterId == data.CharacterId);
            var update = false;

            if (source != null)
            {
                if (data.CharacterName != null && data.CharacterName != source.CharacterName)
                {
                    _context.DataLog.Add(new DataLog
                    {
                        FromValue = source.CharacterName,
                        ToValue = data.CharacterName,
                        Operator = accountId,
                        RecordId = source.CharacterId,
                        TableColumn = GlobalOperation.COLUMN_CHARACTER_NAME,
                        ReportTime = _time.UtcNow
                    });

                    source.CharacterName = data.CharacterName;

                    update = true;
                }

                if (data.AccountId != 0 && data.AccountId != source.AccountId)
                {
                    _context.DataLog.Add(new DataLog
                    {
                        FromValue = source.AccountId.ToString(),
                        ToValue = data.AccountId.ToString(),
                        Operator = accountId,
                        RecordId = source.CharacterId,
                        TableColumn = GlobalOperation.COLUMN_CHARACTER_ACCOUNT,
                        ReportTime = _time.UtcNow
                    });

                    source.AccountId = data.AccountId;

                    update = true;
                }

                if (data.ServiceId != 0 && data.ServiceId != source.ServiceId)
                {
                    _context.DataLog.Add(new DataLog
                    {
                        FromValue = source.ServiceId.ToString(),
                        ToValue = data.ServiceId.ToString(),
                        Operator = accountId,
                        RecordId = source.CharacterId,
                        TableColumn = GlobalOperation.COLUMN_CHARACTER_SERVICE,
                        ReportTime = _time.UtcNow
                    });

                    source.ServiceId = data.ServiceId;

                    update = true;
                }

                if (data.HomeWorld != 0 && data.HomeWorld != source.HomeWorld)
                {
                    _context.DataLog.Add(new DataLog
                    {
                        FromValue = source.HomeWorld.ToString(),
                        ToValue = data.HomeWorld.ToString(),
                        Operator = accountId,
                        RecordId = source.CharacterId,
                        TableColumn = GlobalOperation.COLUMN_CHARACTER_WORLD,
                        ReportTime = _time.UtcNow
                    });

                    source.HomeWorld = data.HomeWorld;

                    update = true;
                }

                if (data.RetainerList != null && !data.RetainerList.SequenceEqual(source.RetainerList))
                {
                    source.RetainerList = data.RetainerList;

                    update = true;
                }

                if (data.JobLevels != null && !data.JobLevels.SequenceEqual(source.JobLevels))
                {
                    source.JobLevels = data.JobLevels;

                    update = true;
                }

                if (data.Inventory != null)
                {
                    source.Inventory = data.Inventory;

                    update = true;
                }

                if (data.GilHold != 0)
                {
                    source.GilHold = data.GilHold;

                    update = true;
                }

                _context.CharacterData.Update(source);
            }

            else
            {
                //source = _context.CharacterData.CreateProxy();
                source = new CharacterData();
                {
                    source.CharacterId = data.CharacterId;
                    source.CharacterName = data.CharacterName;
                    source.ServiceId = data.ServiceId;
                    source.AccountId = data.AccountId;
                    source.HomeWorld = data.HomeWorld;
                    source.RetainerList = data.RetainerList;
                    source.JobLevels = data.JobLevels;
                    source.Inventory = data.Inventory;
                    source.GilHold = data.GilHold;
                }

                _context.CharacterData.Add(source);

                _context.SaveChanges();

                _context.DataLog.Add(new DataLog
                {
                    FromValue = null,
                    ToValue = "INITIAL COMMIT",
                    Operator = accountId,
                    RecordId = source.CharacterId,
                    TableColumn = GlobalOperation.COLUMN_CHARACTER_NEW,
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
