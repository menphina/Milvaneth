using MessagePack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Milvaneth.Common;
using Milvaneth.Common.Communication.Data;
using Milvaneth.Definitions.Communication.Data;
using Milvaneth.Server.Models;
using Milvaneth.Server.Service;
using Milvaneth.Server.Statics;
using Milvaneth.Server.Token;
using Npgsql;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Milvaneth.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataController : ControllerBase
    {
        private ITimeService _time;
        private MilvanethDbContext _context;
        private IAuthentication _auth;
        private ITokenSignService _token;
        private IRepository _repo;

        public DataController(IHttpContextAccessor accessor, ITimeService time, MilvanethDbContext context, IAuthentication auth, IPowService pow, ISrp6Service srp, IApiKeySignService api, ITokenSignService token, IVerifyMailService mail, IRepository repo)
        {
            _time = time;
            _context = context;
            _auth = auth;
            _token = token;
            _repo = repo;
        }

        [Route("overview/{part}")]
        [HttpPost]
        public ActionResult<MilvanethProtocol> DataOverview([FromQuery] string token, int part, MilvanethProtocol data)
        {
            if (!_token.TryDecode(token, out var payload))
            {
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new OverviewResponse
                    {
                        Message = GlobalMessage.OP_TOKEN_NOT_FOUND,
                        ReportTime = _time.SafeNow,
                    }
                };
            }

            if (!(data?.Data is OverviewRequest request) || !request.Check())
            {
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new OverviewResponse
                    {
                        Message = GlobalMessage.DATA_INVALID_INPUT,
                        ReportTime = _time.SafeNow,
                    }
                };
            }

            try
            {
                if (payload.ValidTo < _time.UtcNow)
                {
                    return new MilvanethProtocol
                    {
                        Context = null,
                        Data = new OverviewResponse
                        {
                            Message = GlobalMessage.OP_TOKEN_RENEW_REQUIRED,
                            ReportTime = _time.SafeNow,
                        }
                    };
                }

                _auth.EnsureToken(payload, TokenPurpose.AccessToken, GlobalOperation.DATA_OVERVIEW, -15, out _);

                var query = request.QueryItems.OrderBy(x => x).Skip(part * GlobalConfig.DATA_OVERVIEW_QUERY_LIMIT)
                    .Take(GlobalConfig.DATA_OVERVIEW_QUERY_LIMIT).ToArray();

                var param = new NpgsqlParameter<int[]>("query", query);

                 var result = _context.OverviewData.AsNoTracking()
                    .FromSql(
                        $"select record_id, bucket_id, report_time, world, reporter_id, item_id, open_listing, demand from (select *, rank() over (partition by item_id, world order by report_time desc) as ranking from overview_data where item_id = any(@query)) as result_table where ranking = 1", param).Select(x => x.FromDb()).ToList();

                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new OverviewResponse
                    {
                        Message = GlobalMessage.OK_SUCCESS,
                        ReportTime = _time.SafeNow,
                        EstiTotalParts = request.QueryItems.Length / GlobalConfig.DATA_OVERVIEW_QUERY_LIMIT + 1,
                        FinalPart = part * GlobalConfig.DATA_OVERVIEW_QUERY_LIMIT + query.Count() >= request.QueryItems.Length,
                        PartId = part,
                        Results = result
                    }
                };
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error in DATA/OVERVIEW/{part}");
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new OverviewResponse
                    {
                        Message = GlobalMessage.OP_TOKEN_NOT_FOUND,
                        ReportTime = _time.SafeNow,
                    }
                };
            }
        }

        [Route("item/{id}")]
        [HttpGet]
        public ActionResult<MilvanethProtocol> DataItem([FromQuery] string token, int id)
        {
            if (!_token.TryDecode(token, out var payload))
            {
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new PackedResultBundle
                    {
                        Message = GlobalMessage.OP_TOKEN_NOT_FOUND,
                        ReportTime = _time.SafeNow,
                    }
                };
            }

            try
            {
                if (payload.ValidTo < _time.UtcNow)
                {
                    return new MilvanethProtocol
                    {
                        Context = null,
                        Data = new PackedResultBundle
                        {
                            Message = GlobalMessage.OP_TOKEN_RENEW_REQUIRED,
                            ReportTime = _time.SafeNow,
                        }
                    };
                }

                _auth.EnsureToken(payload, TokenPurpose.AccessToken, GlobalOperation.DATA_ITEM, -25, out _);

                var listing = _context.ListingData.AsNoTracking().FromSql(
                    $"select * from listing_data where bucket_id in (select bucket_id from (select bucket_id, rank() over (partition by world order by report_time desc) as ranking from listing_data where item_id = {id}) as result_table where ranking = 1)");

                var history = _context.HistoryData.AsNoTracking().Where(x => x.ItemId == id).OrderByDescending(x => x.PurchaseTime).Take(100).Select(x => x.FromDb()).ToList();

                var query = listing.ToList().OrderByDescending(x => x.ListingId).Select(x => x.ArtisanId).Take(32768);

                var names = _context.CharacterData.AsNoTracking().Where(x => query.Contains(x.CharacterId))
                    .Select(x => new {x.CharacterId, x.CharacterName})
                    .ToDictionary(x => x.CharacterId, x => x.CharacterName);

                var listingResult = new List<ListingResponseItem>();

                foreach (var item in listing)
                {
                    listingResult.Add(item.FromDb(names.TryGetValue(item.ArtisanId, out var name) ? name : "服务器无记录"));
                }

                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new PackedResultBundle
                    {
                        Message = GlobalMessage.OK_SUCCESS,
                        ReportTime = _time.SafeNow,
                        ItemId = id,
                        Listings = listingResult,
                        Histories = history
                    }
                };
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error in DATA/ITEM/{id}");
                return new MilvanethProtocol
                {
                    Context = null,
                    Data = new PackedResultBundle
                    {
                        Message = GlobalMessage.OP_TOKEN_NOT_FOUND,
                        ReportTime = _time.SafeNow,
                    }
                };
            }
        }

        [Route("upload")]
        [HttpPost]
        public ActionResult DataUpload([FromQuery] string token, MilvanethProtocol data)
        {
            if (!_token.TryDecode(token, out var payload))
            {
                return StatusCode(401);
            }

            if (!(data?.Context).Check() || !(data?.Data is PackedResult result) || !result.Check())
            {
                return StatusCode(400);
            }

            try
            {
                if (payload.ValidTo < _time.UtcNow)
                {
                    return StatusCode(511);
                }

                _auth.EnsureToken(payload, TokenPurpose.AccessToken, GlobalOperation.DATA_UPLOAD, 0, out var account);

                // virtual deduction for token validation operation
                var karmaBefore = account.Karma;
                account.Karma -= 20;

                switch (result.Type)
                {
                    case PackedResultType.Inventory:
                        var inventory = (InventoryResult)result.Result;

                        if (inventory.Context != data.Context.CharacterId)
                            break;

                        _repo.Character.Commit(account.AccountId, new CharacterData
                        {
                            CharacterId = inventory.Context,
                            ServiceId = data.Context.ServiceId,
                            AccountId = account.AccountId,
                            Inventory = LZ4MessagePackSerializer.Serialize(data.Data)
                        }, false);

                        account.Karma += 20;
                        break;

                    case PackedResultType.InventoryNetwork:
                        var inventoryNetwork = (InventoryResult)result.Result;
                        _repo.Retainer.Commit(account.AccountId, new RetainerData
                        {
                            RetainerId = inventoryNetwork.Context,
                            Character = data.Context.CharacterId,
                            World = data.Context.World,
                            Inventory = LZ4MessagePackSerializer.Serialize(data.Data)
                        }, false);

                        account.Karma += 20;
                        break;

                    case PackedResultType.Artisan:
                        var artisan = (ArtisanResult)result.Result;
                        _repo.Character.CommitRange(account.AccountId, artisan.ArtisanList.Select(x => new CharacterData
                        {
                            CharacterId = x.CharacterId,
                            CharacterName = x.IsValid ? x.CharacterName : null,
                            HomeWorld = x.IsValid && !x.FromMemory ? data.Context.World : 0,
                        }), false);

                        account.Karma += 20 + artisan.ArtisanList.Count(x => x.IsValid) * 10 + artisan.ArtisanList.Count(x => !x.IsValid) * 3;
                        break;

                    case PackedResultType.MarketHistory:
                        var marketHistory = (MarketHistoryResult)result.Result;
                        _repo.History.CommitRange(data.Context.CharacterId,
                            marketHistory.HistoryItems.Select(x => x.ToDb(result.ReportTime, data.Context.World)),
                            false);

                        account.Karma += 20 + 30;
                        break;

                    case PackedResultType.MarketListing:
                        var marketListing = (MarketListingResult) result.Result;

                        var artisanFk = marketListing.ListingItems.Select(x => x.ArtisanId);
                        var ownerFk = marketListing.ListingItems.Select(x => new {x.OwnerId, x.PlayerName}).ToImmutableList();

                        _repo.Character.CommitRange(account.AccountId,
                            ownerFk.Where(x => x.OwnerId != 0).GroupBy(x => x.OwnerId).Select(y =>
                            {
                                var x = y.First();
                                return new CharacterData
                                {
                                    CharacterId = x.OwnerId,
                                    HomeWorld = data.Context.World,
                                    CharacterName = x.PlayerName
                                };
                            }),
                            false);

                        _repo.Character.CommitRange(account.AccountId,
                            artisanFk.Where(x => x > 0 && ownerFk.All(y => y.OwnerId != x)).Distinct().Select(x => new CharacterData { CharacterId = x }),
                            true);

                        _repo.Retainer.CommitRange(account.AccountId,
                            marketListing.ListingItems.GroupBy(x => x.RetainerId).Select(y =>
                            {
                                var x = y.First();
                                return new RetainerData
                                    {RetainerId = x.RetainerId, Character = x.OwnerId, RetainerName = x.RetainerName, Location = x.RetainerLocation, World = data.Context.World};
                            }),
                            true);

                        _repo.Listing.CommitRange(data.Context.CharacterId,
                            marketListing.ListingItems.Select(x => x.ToDb(result.ReportTime, data.Context.World)),
                            false);

                        foreach (var x in marketListing.ListingItems.GroupBy(x => x.ItemId))
                        {
                            _repo.Overview.Commit(data.Context.CharacterId,
                                new OverviewData
                                {
                                    ReportTime = result.ReportTime,
                                    World = data.Context.World,
                                    ItemId = x.Key,
                                    OpenListing = (short) x.Count(),
                                }, false);
                        }


                        account.Karma += 20 + 100;
                        break;

                    case PackedResultType.MarketOverview:
                        var marketOverview = (MarketOverviewResult) result.Result;
                        _repo.Overview.CommitRange(data.Context.CharacterId,
                            marketOverview.ResultItems.Select(x => x.ToDb(result.ReportTime, data.Context.World)),
                            false);

                        account.Karma += 20 + 30 + marketOverview.ResultItems.Count;
                        break;

                    case PackedResultType.RetainerHistory:
                        // todo
                        break;

                    case PackedResultType.RetainerList:
                        var retainerList = (RetainerInfoResult) result.Result;
                        _repo.Retainer.CommitRange(account.AccountId,
                            retainerList.RetainerInfo.Select(x => x.ToDb(data.Context.CharacterId, data.Context.World)), false);

                        account.Karma += 20 + retainerList.RetainerInfo.Count * 4;
                        break;

                    case PackedResultType.RetainerUpdate:
                        #region NotFinishedCode
                        // todo

                        // 目前有两种方案，直接更新和Copy-on-Update，前者的主要问题在于并发条件下Time-Bucket体系可能会出现问题，后者则在于性能开销，故目前暂不对此数据进行处理

                        //var retainerUpdate = (RetainerUpdateResult)result.Result;

                        //foreach (var item in retainerUpdate.UpdateItems)
                        //{
                        //    var record = _context.ListingData
                        //        .Where(x => x.RetainerId == item.RetainerId && x.ContainerId == (short)item.ContainerId && x.SlotId == item.ContainerSlot)
                        //        .OrderByDescending(x => x.ReportTime).Take(1);

                        //    if (!record.Any())
                        //    {
                        //        var retInfo =
                        //            _context.RetainerData.SingleOrDefault(x => x.RetainerId == item.RetainerId);

                        //        var record2 = _context.ListingData.Where(x =>
                        //                x.World == data.Context.World && x.ItemId == item.ItemInfo.ItemId &&
                        //                x.ReportTime <= result.ReportTime)
                        //            .OrderByDescending(x => x.ReportTime).Take(1);

                        //        _repo.Commit(data.Context.CharacterId, new ListingData
                        //        {
                        //            BucketId = record2.Any() ? record2.First().BucketId : Guid.Empty,
                        //            ReportTime = result.ReportTime,
                        //            World = data.Context.World,
                        //            ReporterId = data.Context.CharacterId,
                        //            ListingId = 0,
                        //            RetainerId = item.RetainerId,
                        //            OwnerId = data.Context.CharacterId,
                        //            ArtisanId = item.ItemInfo.ArtisanId,
                        //            UnitPrice = item.NewPrice,
                        //            TotalTax = 0,
                        //            Quantity = item.ItemInfo.Amount,
                        //            ItemId = item.ItemInfo.ItemId,
                        //            UpdateTime = Helper.DateTimeToUnixTimeStamp(result.ReportTime),
                        //            ContainerId = (short)item.ContainerId,
                        //            SlotId = (short)item.ContainerSlot,
                        //            Condition = (short)item.ItemInfo.Durability,
                        //            SpiritBond = (short)item.ItemInfo.SpiritBond,
                        //            Materia1 = item.ItemInfo.Materia1,
                        //            Materia2 = item.ItemInfo.Materia2,
                        //            Materia3 = item.ItemInfo.Materia3,
                        //            Materia4 = item.ItemInfo.Materia4,
                        //            Materia5 = item.ItemInfo.Materia5,
                        //            RetainerName = retInfo?.RetainerName,
                        //            PlayerName = item.ContainerId == InventoryContainerId.HIRE_LISTING ? null : _context.CharacterData.SingleOrDefault(x => x.CharacterId == data.Context.CharacterId)?.CharacterName,
                        //            IsHq = item.ItemInfo.IsHq,
                        //            MateriaCount = 0, //todo
                        //            OnMannequin = item.ContainerId != InventoryContainerId.HIRE_LISTING,
                        //            RetainerLoc = 0,//todo
                        //            DyeId = item.ItemInfo.DyeId
                        //        }, true);

                        //        account.Karma += 25;
                        //        continue;
                        //    }

                        //    var recordEntity = record.First();

                        //    if (recordEntity.ReportTime >= result.ReportTime)
                        //    {
                        //        account.Karma -= 25;
                        //        continue;
                        //    }

                        //    recordEntity.
                        //}

                        //account.Karma += 20 + retainerUpdate.UpdateItems.Count * 25;

                        #endregion

                        account.Karma += 20;
                        break;

                    case PackedResultType.Status:
                        var status = (StatusResult) result.Result;

                        if(status.CharacterId != data.Context.CharacterId)
                            break;

                        if (account.PlayedCharacter == null)
                            account.PlayedCharacter = new long[0];

                        if (!account.PlayedCharacter.Contains(status.CharacterId))
                        {
                            var temp = new long[account.PlayedCharacter.Length + 1];
                            Array.Copy(account.PlayedCharacter, temp, account.PlayedCharacter.Length);
                            temp[temp.Length - 1] = status.CharacterId;
                            account.PlayedCharacter = temp;
                        }

                        _repo.Character.Commit(account.AccountId, new CharacterData
                        {
                            CharacterId = status.CharacterId,
                            CharacterName = status.CharacterName,
                            ServiceId = data.Context.ServiceId,
                            AccountId = account.AccountId,
                            HomeWorld = status.CharacterHomeWorld,
                            JobLevels = status.LevelInfo.ToDb(),
                            GilHold = status.CharaInfo.GilHold
                        }, false);

                        account.Karma += 20 + 40;

                        break;

                    case PackedResultType.LobbyService:
                        var lobbyService = (LobbyServiceResult) result.Result;

                        if (lobbyService.ServiceId != data.Context.ServiceId)
                            break;

                        if (account.RelatedService == null)
                            account.RelatedService = new long[0];

                        if (!account.RelatedService.Contains(lobbyService.ServiceId))
                        {
                            var temp = new long[account.RelatedService.Length + 1];
                            Array.Copy(account.RelatedService, temp, account.RelatedService.Length);
                            temp[temp.Length - 1] = lobbyService.ServiceId;
                            account.RelatedService = temp;
                        }

                        account.Karma += 20;
                        break;

                    case PackedResultType.LobbyCharacter:
                        var lobbyCharacter = (LobbyCharacterResult) result.Result;
                        if (!DataChecker.CheckOnlineCharacterBinding(data.Context.ServiceId,
                            lobbyCharacter.CharacterItems))
                        {
#warning api availability is not checked.
                            account.Karma -= 180;
                            break;
                        }

                        if (account.RelatedService == null)
                            account.RelatedService = new long[0];

                        if (!account.RelatedService.Contains(data.Context.ServiceId))
                        {
                            var temp = new long[account.RelatedService.Length + 1];
                            Array.Copy(account.RelatedService, temp, account.RelatedService.Length);
                            temp[temp.Length - 1] = data.Context.ServiceId;
                            account.RelatedService = temp;
                        }

                        _repo.Character.CommitRange(account.AccountId, lobbyCharacter.CharacterItems.Select(x => x.ToDb(data.Context.ServiceId)), false);

                        account.Karma += 20 + 20 * lobbyCharacter.CharacterItems.Count;
                        break;

                    default:
                        // do nothing
                        break;
                }

                _context.KarmaLog.Add(new KarmaLog
                {
                    ReportTime = _time.UtcNow,
                    AccountId = account.AccountId,
                    Reason = GlobalOperation.DATA_UPLOAD + (int) result.Type,
                    Before = karmaBefore,
                    After = account.Karma
                });

                _context.AccountData.Update(account);
                _context.SaveChanges();

                return StatusCode(200);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error in DATA/UPLOAD");
                return StatusCode(500);
            }
        }
    }
}