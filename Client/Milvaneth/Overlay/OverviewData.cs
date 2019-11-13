using Milvaneth.Definitions.Communication.Data;
using System;

namespace Milvaneth.Overlay
{
    public class OverviewData : IMarketData
    {
        public int ItemId { get; set; }
        public int OpenListing { get; set; }
        public int Demand { get; set; }
        public DateTime UpdateTime { get; set; }
        public int Zone { get; set; }
        public int World { get; set; }

        public static OverviewData FromResultItem(OverviewData inherit, OverviewResponseItem item)
        {
            if (inherit == null)
            {
                return new OverviewData
                {
                    ItemId = item.ItemId,
                    OpenListing = item.OpenListing,
                    Demand = item.Demand,
                    UpdateTime = item.ReportTime,
                    World = item.World,
                };
            }

            if (item.ItemId != inherit.ItemId)
                return inherit;

            return new OverviewData
            {
                ItemId = item.ItemId,
                OpenListing = item.OpenListing + inherit.OpenListing,
                Demand = item.Demand + inherit.Demand,
                UpdateTime = item.ReportTime > inherit.UpdateTime ? item.ReportTime.LocalTime : inherit.UpdateTime,
                World = 0,
            };
        }
    }
}
