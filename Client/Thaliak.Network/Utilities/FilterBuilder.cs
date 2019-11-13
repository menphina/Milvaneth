using System.Collections.Generic;
using Thaliak.Network.Filter;
using Thaliak.Network.Sniffer;

namespace Thaliak.Network.Utilities
{
    public class FilterBuilder
    {
        public static Filters<IPPacket> BuildDefaultFilter(List<Connection> conn)
        {
            var filters = new Filters<IPPacket>(FilterOperator.OR);

            for (var i = 0; i < conn.Count; i++)
            {
                filters.PropertyFilters.Add(new PropertyFilter<IPPacket>(x => x.Connection, conn[i],
                    MessageAttribute.DirectionSend | i.ToPort())); // C2S
                filters.PropertyFilters.Add(new PropertyFilter<IPPacket>(x => x.Connection, conn[i].Reverse,
                    MessageAttribute.DirectionReceive | i.ToPort())); // S2C
            }

            return filters;
        }

        public static Filters<IPPacket> BuildDefaultFilter(List<Connection> lobby, List<Connection> game)
        {
            var filters = new Filters<IPPacket>(FilterOperator.OR);

            for (var i = 0; i < lobby.Count; i++)
            {
                filters.PropertyFilters.Add(new PropertyFilter<IPPacket>(x => x.Connection, lobby[i],
                    MessageAttribute.CatalogLobby | MessageAttribute.DirectionSend | i.ToPort())); // C2S
                filters.PropertyFilters.Add(new PropertyFilter<IPPacket>(x => x.Connection, lobby[i].Reverse,
                    MessageAttribute.CatalogLobby | MessageAttribute.DirectionReceive | i.ToPort())); // S2C
            }

            for (var i = 0; i < game.Count; i++)
            {
                filters.PropertyFilters.Add(new PropertyFilter<IPPacket>(x => x.Connection, game[i],
                    MessageAttribute.CatalogWorld | MessageAttribute.DirectionSend | i.ToPort())); // C2S
                filters.PropertyFilters.Add(new PropertyFilter<IPPacket>(x => x.Connection, game[i].Reverse,
                    MessageAttribute.CatalogWorld | MessageAttribute.DirectionReceive | i.ToPort())); // S2C
            }

            return filters;
        }
    }
}
