using Milvaneth.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Milvaneth.Communication.Data
{
    internal class DataContainer
    {
        public MilvanethContext Context;
        private readonly ConcurrentQueue<MilvanethProtocol> _dataQueue;
        private RetainerInfoResult _rirOriginal;
        public DataContainer(ConcurrentQueue<MilvanethProtocol> queue)
        {
            _dataQueue = queue;

            Context = new MilvanethContext();
            _rirOriginal = new RetainerInfoResult(new List<RetainerInfoItem>());
        }

        public bool TryAdd(PackedResult pr)
        {
            switch (pr.Type)
            {
                // ignoring
                case PackedResultType.Chatlog:
                    return false;
                case PackedResultType.MarketRequest:
                    return false;


                // acknowledging
                case PackedResultType.CurrentWorld:
                    if (!(pr.Result is CurrentWorldResult cwr))
                        throw new InvalidCastException("Failed to convert type");

                    Context.World = cwr.WorldId;
                    return true;

                // send on diff
                case PackedResultType.RetainerList:
                    if (!(pr.Result is RetainerInfoResult rir))
                        throw new InvalidCastException("Failed to convert type");

                    if (_rirOriginal.RetainerInfo.Select(x => x.RetainerName)
                        .SequenceEqual(rir.RetainerInfo.Select(x => x.RetainerName)))
                        return true;

                    _rirOriginal = rir;
                    break;

                // contexting
                case PackedResultType.LobbyService:
                    if (!(pr.Result is LobbyServiceResult lsr))
                        throw new InvalidCastException("Failed to convert type");

                    Context.ServiceId = lsr.ServiceId;
                    break;

                case PackedResultType.Status:
                    if (!(pr.Result is StatusResult sr))
                        throw new InvalidCastException("Failed to convert type");

                    Context.World = sr.CharacterCurrentWorld;
                    Context.CharacterId = sr.CharacterId;
                    Context.ConnectionNumber = Environment.TickCount;
                    break;

                // direct send
                case PackedResultType.Artisan:
                    break;
                case PackedResultType.Inventory:
                    break;
                case PackedResultType.InventoryNetwork:
                    break;
                case PackedResultType.LobbyCharacter:
                    break;
                case PackedResultType.MarketHistory:
                    break;
                case PackedResultType.MarketListing:
                    break;
                case PackedResultType.MarketOverview:
                    break;
                case PackedResultType.RetainerHistory:
                    break;
                case PackedResultType.RetainerUpdate:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //if (Context.CharacterId == 0 || Context.World == 0) return false;
            _dataQueue.Enqueue(new MilvanethProtocol{Context = Context.Copy(), Data = pr});
            return true;
        }
    }
}
