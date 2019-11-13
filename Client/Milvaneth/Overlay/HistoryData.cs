using System;
using Milvaneth.Common;

namespace Milvaneth.Overlay
{
    public class HistoryData : IMarketData
    {
        public int ItemId { get; set; }
        public int UnitPrice { get; set; }
        public int Quantity { get; set; }
        public bool IsHq { get; set; }
        public bool OnMannequin { get; set; }
        public string BuyerName { get; set; }
        public DateTime PurchaseTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public int Zone { get; set; }
        public int World { get; set; }

        public static HistoryData FromResultItem(MarketHistoryItem item, DateTime time, int zone, int world)
        {
            return new HistoryData
            {
                ItemId = item.ItemId,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                IsHq = item.IsHq != 0,
                OnMannequin = item.OnMannequin != 0,
                BuyerName = item.BuyerName,
                PurchaseTime = Helper.UnixTimeStampToDateTime(item.PurchaseTime),
                UpdateTime = time,
                Zone = zone,
                World = world,
            };
        }
    }
}
