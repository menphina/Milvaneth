using Milvaneth.Common;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Thaliak.Network;
using Thaliak.Network.Messages;

namespace Milvaneth.Cmd
{
    internal class GameDataManager
    {
        public delegate void GameLoggingOutDelegate();
        public delegate void DataOutputDelegate(PackedResult pr);
        public delegate InventoryResult RequestScanDelegate(InventoryContainerId type);

        public GameLoggingOutDelegate OnGameLoggingOut;
        public DataOutputDelegate OnDataReady;
        public RequestScanDelegate OnRequestScan;

        private List<ArtisanInfo> _crBuffer;
        private List<RetainerInfoItem> _riBuffer;
        private List<MarketListingItem> _mlBuffer;
        private List<MarketOverviewItem> _loBuffer;
        private List<LobbyCharacterItem> _lcBuffer;
        private Dictionary<int, InventoryContainer> _irBuffer;
        private Dictionary<int, int> _ipBuffer;
        private List<int> _lpBuffer;
        private int _listingPacketCount;
        private short _listingReqId;
        private int _overviewReqId;
        private int _worldId;
        private long _retainerId;
        private int _retainerCountdown;

        private CountdownEvent ceOverview;
        private bool _started;
        private bool _blocked;

        private const int PrimeForInventorySlot = 241;

        public GameDataManager()
        {
            _crBuffer = new List<ArtisanInfo>();
            _riBuffer = new List<RetainerInfoItem>();
            _mlBuffer = new List<MarketListingItem>();
            _loBuffer = new List<MarketOverviewItem>();
            _lcBuffer = new List<LobbyCharacterItem>();
            _irBuffer = new Dictionary<int, InventoryContainer>();
            _ipBuffer = new Dictionary<int, int>();
            _lpBuffer = new List<int>();

            ceOverview = new CountdownEvent(2);
        }

        public void Start()
        {
            Log.Debug("GDM Start");
            _started = true;
            _blocked = false;
            RunOverviewTask();
        }

        public void Stop()
        {
            Log.Debug("GDM Stop");
            if (_crBuffer.Any())
            {
                OnDataReady?.Invoke(new PackedResult(PackedResultType.Artisan,
                    new ArtisanResult(new List<ArtisanInfo>(_crBuffer))));
                _crBuffer.Clear();
            }

            _started = false;
            _blocked = true;
            ceOverview.Reset(0);
        }

        public void Dispose()
        {
            Log.Debug("GDM Dispose");
            ceOverview.Dispose();
        }

        public void ScheduledTasks()
        {
            if (!_started || _blocked) return;

            ceOverview.Signal();
        }

        public void HandleNetworkCharacterName(NetworkMessageHeader header, IResult message)
        {
            var dat = (NetworkCharacterName) message;
            _crBuffer.Add(new ArtisanInfo(dat.CharacterId, dat.Name, false));

            if (_crBuffer.Count < 5) return;

            OnDataReady?.Invoke(new PackedResult(PackedResultType.Artisan,
                new ArtisanResult(new List<ArtisanInfo>(_crBuffer))));
            _crBuffer.Clear();
        }

        public void HandleRequestRetainer(NetworkMessageHeader header, IResult message)
        {
            if (message == null) return;

            var dat = (NetworkRequestRetainer) message;
            _retainerId = dat.RetainerId;
            _retainerCountdown = 1;
            _irBuffer.Clear();
            _ipBuffer.Clear();
        }

        public void HandleNetworkItemInfo(NetworkMessageHeader header, IResult message)
        {
            if (_retainerCountdown <= 0) return;

            var dat = (NetworkItemInfo) message;
            if (!_irBuffer.ContainsKey(dat.ContainerId))
            {
                _irBuffer[dat.ContainerId] =
                    new InventoryContainer(InventoryContainerTypeConverter.ToOffset(dat.ContainerId), (InventoryContainerId)dat.ContainerId);
            }

            var item = new InventoryItem(dat.Quantity, dat.Condition, dat.GlamourItemId, dat.ItemId,
                dat.IsHq, dat.SpiritBond, dat.ContainerSlot, dat.ArtisanId, dat.DyeId,
                dat.Materia1, dat.Materia2, dat.Materia3, dat.Materia4, dat.Materia5);

            if (_ipBuffer.ContainsKey(dat.ContainerId * PrimeForInventorySlot + dat.ContainerSlot)) // max slot is 240, also 241 is a prime
            {
                item.UnitPrice = _ipBuffer[dat.ContainerId * PrimeForInventorySlot + dat.ContainerSlot];
            }

            _irBuffer[dat.ContainerId].InventoryItems.Add(item);
        }

        public void HandleNetworkItemPriceInfo(NetworkMessageHeader header, IResult message)
        {
            if (_retainerCountdown <= 0) return;

            var dat = (NetworkItemPriceInfo) message;
            if (!_irBuffer.ContainsKey(dat.ContainerId))
            {
                _ipBuffer[dat.ContainerId * PrimeForInventorySlot + dat.ContainerSlot] = dat.UnitPrice;
                return;
            }

            if (!_irBuffer[dat.ContainerId].InventoryItems.Exists(x => x.Slot == dat.ContainerSlot))
            {
                _ipBuffer[dat.ContainerId * PrimeForInventorySlot + dat.ContainerSlot] = dat.UnitPrice;
                return;
            }

            _irBuffer[dat.ContainerId].InventoryItems.Where(x => x.Slot == dat.ContainerSlot).ToList()
                .ForEach(x => x.UnitPrice = dat.UnitPrice);
        }

        public void HandleNetworkItemInfoEnd(NetworkMessageHeader header, IResult message)
        {
            _retainerCountdown--;

            if (_retainerCountdown != 0)
                return;

            OnDataReady?.Invoke(new PackedResult(PackedResultType.InventoryNetwork,
                new InventoryResult(_irBuffer.Select(x => x.Value).ToList(), _retainerId)));
        }

        public void HandleNetworkLogout(NetworkMessageHeader header, IResult message)
        {
            OnGameLoggingOut?.Invoke();
        }

        public void HandleNetworkMarketHistory(NetworkMessageHeader header, IResult message)
        {
            OnDataReady?.Invoke(new PackedResult(PackedResultType.MarketHistory, (MarketHistoryResult) message));
        }

        public void HandleNetworkMarketListingCount(NetworkMessageHeader header, IResult message)
        {
            var dat = (NetworkMarketListingCount) message;
            _listingPacketCount = dat.Quantity / 10;
            if (dat.Quantity % 10 != 0) _listingPacketCount++;
            OnDataReady?.Invoke(new PackedResult(PackedResultType.MarketRequest, new MarketRequestResult {ItemId = dat.ItemId}));
        }

        public void HandleNetworkMarketListing(NetworkMessageHeader header, IResult message)
        {
            var dat = (MarketListingResult) message;

            _listingReqId = dat.RequestId;
            _mlBuffer.AddRange(dat.ListingItems);
            _listingPacketCount--;

            if (_listingPacketCount > 0) return;

            OnDataReady?.Invoke(new PackedResult(PackedResultType.MarketListing, new MarketListingResult
            {
                ListingItems = new List<MarketListingItem>(_mlBuffer),
                RequestId = _listingReqId
            }));
            _listingReqId = -1;
            _mlBuffer.Clear();
        }

        public void HandleNetworkMarketResult(NetworkMessageHeader header, IResult message)
        {
            _blocked = false;

            var dat = (MarketOverviewResult) message;
            if (dat.RequestId != _overviewReqId)
            {
                ceOverview.Reset(0);

                _overviewReqId = dat.RequestId;
            }

            ceOverview.Reset(2);
            _loBuffer.AddRange(dat.ResultItems);
        }

        public void HandleNetworkRetainerHistory(NetworkMessageHeader header, IResult message)
        {
            OnDataReady?.Invoke(new PackedResult(PackedResultType.RetainerHistory, (RetainerHistoryResult) message));
        }

        public void HandleNetworkRetainerSummary(NetworkMessageHeader header, IResult message)
        {
            var dat = (NetworkRetainerSummary) message;

            if (dat.RetainerId == 0) return;

            _riBuffer.Add(new RetainerInfoItem
            {
                RetainerId = dat.RetainerId,
                RetainerOrder = dat.RetainerOrder,
                ItemsInSell = dat.ItemInSell,
                RetainerLocation = dat.RetainerLocation,
                ListingDueDate = dat.ListingDueDate,
                RetainerName = dat.RetainerName,
            });
        }

        public void HandleNetworkRetainerSumEnd(NetworkMessageHeader header, IResult message)
        {
            OnDataReady?.Invoke(new PackedResult(PackedResultType.RetainerList,
                new RetainerInfoResult(new List<RetainerInfoItem>(_riBuffer))));
            _riBuffer.Clear();
        }

        public void HandleNetworkPlayerSpawn(NetworkMessageHeader header, IResult message)
        {
            var dat = (NetworkPlayerSpawn) message;

            if (_worldId == dat.CurrentWorldId) return;

            _worldId = dat.CurrentWorldId;
            OnDataReady?.Invoke(new PackedResult(PackedResultType.CurrentWorld,
                new CurrentWorldResult(dat.CurrentWorldId)));
        }

        public void HandleClientTrigger(NetworkMessageHeader header, IResult message)
        {
            if (message == null) return;

            var msg = (NetworkClientTrigger) message;
            
            var dat = new RetainerUpdateItem();
            dat.ContainerId = InventoryContainerId.HIRE_LISTING;
            dat.NewPrice = msg.Param12;
            dat.ContainerSlot = msg.Param11;
            dat.IsRemove = false;
            try
            {
                dat.RetainerId = _retainerId;
                dat.ItemInfo = _irBuffer[(int)dat.ContainerId].InventoryItems.First(x => x.Slot == dat.ContainerSlot);
                dat.OldPrice =
                    _ipBuffer.TryGetValue((int) dat.ContainerId * PrimeForInventorySlot + dat.ContainerSlot,
                        out var old)
                        ? old
                        : dat.ItemInfo.UnitPrice;
                dat.ItemInfo.UnitPrice = dat.NewPrice;
            }
            catch
            {
                dat.RetainerId = -1;
                dat.ItemInfo = null;
                dat.OldPrice = -1;
            }

            OnDataReady?.Invoke(new PackedResult(PackedResultType.RetainerUpdate,
                new RetainerUpdateResult {UpdateItems = new List<RetainerUpdateItem> {dat}})); // add or change
        }

        public void HandleInventoryModify(NetworkMessageHeader header, IResult message)
        {
            if (message == null) return;

            var msg = (NetworkInventoryModify) message;
            if (msg.FromContainer == InventoryContainerId.HIRE_LISTING)
            {
                if (_irBuffer.TryGetValue((int) msg.FromContainer, out var items))
                {
                    items.InventoryItems.RemoveAll(x => x.Slot == msg.FromSlot);
                }

                var dat = new RetainerUpdateItem();
                dat.ContainerId = InventoryContainerId.HIRE_LISTING;
                dat.ContainerSlot = msg.FromSlot;
                dat.IsRemove = true;

                OnDataReady?.Invoke(new PackedResult(PackedResultType.RetainerUpdate, 
                    new RetainerUpdateResult { UpdateItems = new List<RetainerUpdateItem> { dat } })); // delete
            }
            else if (msg.ToContainer == InventoryContainerId.HIRE_LISTING)
            {
                var result = OnRequestScan?.Invoke(InventoryContainerId.HIRE_LISTING);
                if (result == null) return;
                if (!_irBuffer.TryGetValue((int) msg.ToContainer, out var items))
                {
                    _irBuffer[(int)msg.ToContainer] = new InventoryContainer(
                        InventoryContainerTypeConverter.ToOffset((int)InventoryContainerId.HIRE_LISTING),
                        InventoryContainerId.HIRE_LISTING);
                }

                items.InventoryItems.AddRange(result.InventoryContainers.First().InventoryItems.Where(x => x.Slot == msg.ToSlot));
                // always a ClientTrigger follows add operation, so we do not need to forge request here.
            }
        }

        public void HandleLobbyService(NetworkMessageHeader header, IResult message)
        {
            OnDataReady?.Invoke(new PackedResult(PackedResultType.LobbyService, (LobbyServiceResult) message));
        }

        public void HandleLobbyCharacter(NetworkMessageHeader header, IResult message)
        {
            var dat = (LobbyCharacterResult) message;

            if (_lpBuffer.Contains(dat.MessageCounter)) return;

            _lpBuffer.Add(dat.MessageCounter);
            _lcBuffer.AddRange(dat.CharacterItems);

            if (dat.MessageCounter % 4 == 0) return;

            OnDataReady?.Invoke(new PackedResult(PackedResultType.LobbyCharacter, new LobbyCharacterResult
            {
                CharacterItems = new List<LobbyCharacterItem>(_lcBuffer),
                MessageCount = (byte)_lcBuffer.Count,
                MessageCounter = dat.MessageCounter,
            }));

            _lpBuffer.Clear();
            _lcBuffer.Clear();
        }

        private void RunOverviewTask()
        {
            Task.Run(() =>
            {
                for (;;)
                {
                    _blocked = true;
                    ceOverview.Reset(2);

                    if (_started)
                    {
                        ceOverview.Wait();

                        if (_loBuffer.Count != 0)
                        {
                            OnDataReady?.Invoke(new PackedResult(PackedResultType.MarketOverview,
                                new MarketOverviewResult
                                {
                                    RequestId = _overviewReqId,
                                    ResultItems = new List<MarketOverviewItem>(_loBuffer)
                                }));

                            _loBuffer.Clear();
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            });
        }
    }
}
