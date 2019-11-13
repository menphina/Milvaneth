using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media.Imaging;
using Milvaneth.Service;

namespace Milvaneth.Overlay
{
    public class OverlayPresenterViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private BitmapSource _itemIcon;
        private string _itemName;
        private string _formattedMinPrice;
        private string _formattedUpdateInfo;
        private string _formattedListInfo;
        private string _formattedHistInfo;

        public BitmapSource ItemIcon
        {
            get => _itemIcon;
            set
            {
                _itemIcon = value;
                OnPropertyChanged(nameof(ItemIcon));
            }
        }
        public string ItemName
        {
            get => _itemName;
            set
            {
                _itemName = value;
                OnPropertyChanged(nameof(ItemName));
            }
        }
        public string FormattedMinPrice
        {
            get => _formattedMinPrice;
            set
            {
                _formattedMinPrice = value;
                OnPropertyChanged(nameof(FormattedMinPrice));
            }
        }
        public string FormattedUpdateInfo
        {
            get => _formattedUpdateInfo;
            set
            {
                _formattedUpdateInfo = value;
                OnPropertyChanged(nameof(FormattedUpdateInfo));
            }
        }
        public string FormattedListInfo
        {
            get => _formattedListInfo;
            set
            {
                _formattedListInfo = value;
                OnPropertyChanged(nameof(FormattedListInfo));
            }
        }
        public string FormattedHistInfo
        {
            get => _formattedHistInfo;
            set
            {
                _formattedHistInfo = value;
                OnPropertyChanged(nameof(FormattedHistInfo));
            }
        }

        internal void LoadTestData()
        {
#pragma warning disable CS0618
            var id = 2714;
            var hq = true;
            var od = new List<OverviewData>
            {
                new OverviewData()
                {
                    Demand = 17,
                    ItemId = 2715,
                    OpenListing = 999,
                    UpdateTime = DateTime.Now,
                    World = 1106,
                    Zone = 1,
                },
                new OverviewData()
                {
                    Demand = 9999, // to large
                    ItemId = 929101, // invalid
                    OpenListing = 1000, // too large
                    UpdateTime = DateTime.Now.AddDays(1), // future
                    World = 1103,
                    Zone = 1,
                },
                new OverviewData()
                {
                    Demand = 0, // no demand
                    ItemId = 1, // valid but not for sale
                    OpenListing = 0, // no sale
                    UpdateTime = DateTime.Now.AddDays(-998), // valid
                    World = 1101,
                    Zone = 1,
                },
                new OverviewData()
                {
                    Demand = -3, // invalid
                    ItemId = -2, // invald
                    OpenListing = -2, // invalid
                    UpdateTime = DateTime.Now.AddDays(-1222), // too old
                    World = 1100,
                    Zone = 2,
                },
                new OverviewData()
                {
                    Demand = 17,
                    ItemId = 2715,
                    OpenListing = 100,
                    UpdateTime = DateTime.Now,
                    World = 1099,
                    Zone = 2,
                },
            };
            var ld = new List<ListingData>
            {
                new ListingData
                {
                    Artisan = "工匠",
                    DyeId = 0,
                    IsHq = true,
                    ItemId = 2714,
                    ListingTime = DateTime.Now,
                    Materia = new[] {0x192, 0x192, 0x192, 0x192, 0x192},
                    OnMannequin = false,
                    PlayerName = "",
                    Quantity = 12,
                    Retainer = "雇员",
                    RetainerLocation = 2,
                    UnitPrice = 1999,
                    UpdateTime = DateTime.Now,
                    Zone = 1,
                    World = 1106,
                },
                new ListingData
                {
                    Artisan = "名字超长的工匠", // super long
                    DyeId = -1, // invalid
                    IsHq = false,
                    ItemId = 2714,
                    ListingTime = DateTime.Now.AddDays(-0.5), // 1d
                    Materia = new[] {0x133, 0x134, 0x135, 0x136, 0x137}, // super long
                    OnMannequin = false,
                    PlayerName = "不应显示", // hidden
                    Quantity = 99999, // too much
                    Retainer = "名字超长的雇员", // super long
                    RetainerLocation = 8, // invalid
                    UnitPrice = 1999,
                    Tax = 99999, // hidden
                    UpdateTime = DateTime.Now,
                    Zone = 1,
                    World = 1105,
                },
                new ListingData
                {
                    Artisan = null, // null
                    DyeId = 2, // dyed
                    IsHq = true,
                    ItemId = 2714,
                    ListingTime = DateTime.Now.AddDays(-0.5),
                    Materia = new[] {0, 0, 0, 0, 0}, // no mat
                    OnMannequin = true, // on manne
                    PlayerName = null, // null
                    Quantity = 999,
                    Retainer = null, // null
                    RetainerLocation = 7,
                    UnitPrice = 1999999999, // too large
                    UpdateTime = DateTime.Now,
                    Zone = 1,
                    World = 1106,
                },
                new ListingData
                {
                    Artisan = "工匠",
                    DyeId = 22, // dyed
                    IsHq = false,
                    ItemId = 2714,
                    ListingTime = DateTime.Now.AddDays(-1.5),
                    Materia = new[] {0x131, 0x132, 0x133}, // short
                    OnMannequin = true, // on manne
                    PlayerName = "人偶", // null
                    Quantity = 999,
                    Retainer = "雇员", // null
                    RetainerLocation = 7,
                    UnitPrice = -7, // invalid
                    UpdateTime = DateTime.Now.AddDays(1), // future
                    Zone = 1,
                    World = 1106,
                },
                new ListingData
                {
                    Artisan = "工匠",
                    DyeId = 22, // dyed
                    IsHq = false,
                    ItemId = 2714,
                    ListingTime = DateTime.Now.AddDays(-1.5),
                    Materia = new[] { 0x131, 0x132, 0x133, 0x134, 0x135, 0x136, 0x137}, // long
                    OnMannequin = true, // on manne
                    PlayerName = "人偶", // null
                    Quantity = 999,
                    Retainer = "雇员", // null
                    RetainerLocation = 7,
                    UnitPrice = 1234,
                    UpdateTime = DateTime.Now.AddDays(-998), // valid
                    Zone = 1,
                    World = 1106,
                },
                new ListingData
                {
                    Artisan = "工匠",
                    DyeId = 22, // dyed
                    IsHq = false,
                    ItemId = 2714,
                    ListingTime = DateTime.Now.AddDays(-1.5),
                    Materia = new int[0], // nothing
                    OnMannequin = true, // on manne
                    PlayerName = "人偶", // null
                    Quantity = 0, // invalid
                    Retainer = "雇员", // null
                    RetainerLocation = 10,
                    UnitPrice = 1999999999, // too large
                    UpdateTime = DateTime.Now.AddDays(-24),
                    Zone = 1,
                    World = 1106,
                },
                new ListingData
                {
                    Artisan = "不应显示",
                    DyeId = -1,
                    IsHq = false,
                    ItemId = 27124, // invalid
                    ListingTime = DateTime.Now.AddDays(-0.5),
                    Materia = new[] {0x133, 0x134, 0x135, 0x136, 0x137},
                    OnMannequin = false,
                    PlayerName = "不应显示",
                    Quantity = 12,
                    Retainer = "不应显示",
                    RetainerLocation = 2,
                    UnitPrice = 1999,
                    Tax = 99999999,
                    UpdateTime = DateTime.Now,
                    Zone = 1,
                    World = 1106,
                },
            };
            var hd = new List<HistoryData>
            {

            };
#pragma warning restore CS0618
            UpdateData(id, od, ld, hd, hq);
        }

        internal void UpdateData(int itemId, List<OverviewData> od, List<ListingData> ld, List<HistoryData> hd, bool dispHq = false, bool updHist = false)
        {
            ld = ld?.Where(x => x.ItemId == itemId).ToList() ?? new List<ListingData>();
            hd = hd?.Where(x => x.ItemId == itemId).ToList() ?? new List<HistoryData>();

            ItemIcon = IconManagementService.GetIcon(itemId, dispHq);
            ItemName = itemId != 0 ? DictionaryManagementService.GetName(itemId) : "等待数据";

            var ldNq = ld.Where(x => !x.IsHq);
            var ldHq = ld.Where(x => x.IsHq);
            FormattedMinPrice =
                $"最低 NQ {(ldNq.Count() != 0 ? ldNq.Min(x => x.UnitPrice).ToString("N0") : "无数据")} g" +
                $" / HQ {(ldHq.Count() != 0 ? ldHq.Min(x => x.UnitPrice).ToString("N0") : "无数据")} g";

            var dtn = DateTime.Now;

            var dtm = dtn.AddDays(-999);
            var total = 0d;
            var count = 0;

            var tmp = updHist ? hd.Cast<IMarketData>() : ld;
            foreach (var d in tmp)
            {
                var td = (dtn - d.UpdateTime).TotalDays;

                if (td > 999) // assume user local time is not reliable
                    continue;

                dtm = DateTime.Compare(dtm, d.UpdateTime) > 0 ? dtm : d.UpdateTime;
                total += td;
                count++;
            }

            if ((dtn - dtm).TotalDays >= 999)
            {
                FormattedUpdateInfo =
                    $"最后更新于 ?? 月 ?? 日 / 平均更新时间 999 天";
            }
            else
            {
                FormattedUpdateInfo =
                    $"最后更新于 {dtm.Month:D2} 月 {dtm.Day:D2} 日 / 平均更新时间 {(int)(count != 0 ? total / count : 999)} 天";
            }

            var ldnq = ld.Where(x => !x.IsHq).RemoveOutliers();
            var ldhq = ld.Where(x => x.IsHq).RemoveOutliers();
            var lds = ldhq.Concat(ldnq);
            double ld1ttl = 0d, ld7ttl = 0d, ld30ttl = 0d, ld365ttl = 0d, ldallttl = 0d;
            int ld1cnt = 0, ld7cnt = 0, ld30cnt = 0, ld365cnt = 0, ldallcnt = 0;
            foreach (var s in lds)
            {
                var td = (dtn - s.ListingTime).TotalDays;

                if (td < 0)
                    continue;

                if (td <= 1)
                {
                    ld1ttl += s.UnitPrice;
                    ld1cnt++;
                }

                if (td <= 7)
                {
                    ld7ttl += s.UnitPrice;
                    ld7cnt++;
                }

                if (td <= 30)
                {
                    ld30ttl += s.UnitPrice;
                    ld30cnt++;
                }

                if (td <= 365)
                {
                    ld365ttl += s.UnitPrice;
                    ld365cnt++;
                }

                ldallttl += s.UnitPrice;
                ldallcnt++;
            }

            if (ld1cnt > 10) // day avg. > 10
            {
                FormattedListInfo = $"在售 - 日 {(ld1cnt != 0 ? ((int)(ld1ttl / ld1cnt)).ToString("N0") : "无数据")} g @{ld1cnt:N0}" +
                                    $" / 周 {(ld7cnt != 0 ? ((int)(ld7ttl / ld7cnt)).ToString("N0") : "无数据")} g @{ld7cnt:N0}" +
                                    $" / 月 {(ld30cnt != 0 ? ((int)(ld30ttl / ld30cnt)).ToString("N0") : "无数据")} g @{ld30cnt:N0}";
            }
            else if (ld7cnt < 10) // day avg. < 1
            {
                FormattedListInfo = $"在售 - 月 {(ld30cnt != 0 ? ((int)(ld30ttl / ld30cnt)).ToString("N0") : "无数据")} g @{ld30cnt:N0}" +
                                    $" / 年 {(ld365cnt != 0 ? ((int)(ld365ttl / ld365cnt)).ToString("N0") : "无数据")} g @{ld365cnt:N0}" +
                                    $" / 全 {(ldallcnt != 0 ? ((int)(ldallttl / ldallcnt)).ToString("N0") : "无数据")} g @{ldallcnt:N0}";
            }
            else
            {
                FormattedListInfo = $"在售 - 周 {(ld7cnt != 0 ? ((int)(ld7ttl / ld7cnt)).ToString("N0") : "无数据")} g @{ld7cnt:N0}" +
                                    $" / 月 {(ld30cnt != 0 ? ((int)(ld30ttl / ld30cnt)).ToString("N0") : "无数据")} g @{ld30cnt:N0}" +
                                    $" / 年 {(ld365cnt != 0 ? ((int)(ld365ttl / ld365cnt)).ToString("N0") : "无数据")} g @{ld365cnt:N0}";
            }

            var hds = hd.OrderBy(x => x.PurchaseTime);
            double hd1ttl = 0d, hd7ttl = 0d, hd30ttl = 0d, hd365ttl = 0d, hdallttl = 0d;
            int hd1cnt = 0, hd7cnt = 0, hd30cnt = 0, hd365cnt = 0, hdallcnt = 0;
            foreach (var s in hds)
            {
                var td = (dtn - s.PurchaseTime).TotalDays;

                if (td < 0)
                    continue;

                if (td <= 1)
                {
                    hd1ttl += s.UnitPrice;
                    hd1cnt++;
                }

                if (td <= 7)
                {
                    hd7ttl += s.UnitPrice;
                    hd7cnt++;
                }

                if (td <= 30)
                {
                    hd30ttl += s.UnitPrice;
                    hd30cnt++;
                }

                if (td <= 365)
                {
                    hd365ttl += s.UnitPrice;
                    hd365cnt++;
                }

                if (td < 999)
                {
                    hdallttl += s.UnitPrice;
                    hdallcnt++;
                }
            }

            if (hd1cnt > 10) // day avg. > 10
            {
                FormattedHistInfo = $"成交 - 日 {(hd1cnt != 0 ? ((int)(hd1ttl / hd1cnt)).ToString("N0") : "无数据")} g @{hd1cnt:N0}" +
                                    $" / 周 {(hd7cnt != 0 ? ((int)(hd7ttl / hd7cnt)).ToString("N0") : "无数据")} g @{hd7cnt:N0}" +
                                    $" / 月 {(hd30cnt != 0 ? ((int)(hd30ttl / hd30cnt)).ToString("N0") : "无数据")} g @{hd30cnt:N0}";
            }
            else if (hd7cnt < 7) // day avg. < 1
            {
                FormattedHistInfo = $"成交 - 月 {(hd30cnt != 0 ? ((int)(hd30ttl / hd30cnt)).ToString("N0") : "无数据")} g @{hd30cnt:N0}" +
                                    $" / 年 {(hd365cnt != 0 ? ((int)(hd365ttl / hd365cnt)).ToString("N0") : "无数据")} g @{hd365cnt:N0}" +
                                    $" / 全 {(hdallcnt != 0 ? ((int)(hdallttl / hdallcnt)).ToString("N0") : "无数据")} g @{hdallcnt:N0}";
            }
            else
            {
                FormattedHistInfo = $"成交 - 周 {(hd7cnt != 0 ? ((int)(hd7ttl / hd7cnt)).ToString("N0") : "无数据")} g @{hd7cnt:N0}" +
                                    $" / 月 {(hd30cnt != 0 ? ((int)(hd30ttl / hd30cnt)).ToString("N0") : "无数据")} g @{hd30cnt:N0}" +
                                    $" / 年 {(hd365cnt != 0 ? ((int)(hd365ttl / hd365cnt)).ToString("N0") : "无数据")} g @{hd365cnt:N0}";
            }
        }

        internal void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    internal static class OutliersRemover
    {
        public static IEnumerable<ListingData> RemoveOutliers(this IEnumerable<ListingData> ld)
        {
            if (!ld.Any()) return ld;

            const decimal threshold = 10;
            var med = GetMedian(ld.Select(x => x.UnitPrice).ToArray());
            var mad = GetMedian(ld.Select(x => Math.Abs(med - x.UnitPrice)).ToArray());
            return ld.Where(x => (0.6745m * (Math.Abs(med - x.UnitPrice))) <= (threshold * mad));
        }

        private static decimal GetMedian(int[] array)
        {
            var tempArray = array;
            var count = tempArray.Length;

            Array.Sort(tempArray);

            decimal medianValue = 0;

            if (count % 2 == 0)
            {
                // count is even, need to get the middle two elements, add them together, then divide by 2
                var middleElement1 = tempArray[(count / 2) - 1];
                var middleElement2 = tempArray[(count / 2)];
                medianValue = (middleElement1 + middleElement2) / 2;
            }
            else
            {
                // count is odd, simply get the middle element.
                medianValue = tempArray[(count / 2)];
            }

            return medianValue;
        }

        private static decimal GetMedian(decimal[] array)
        {
            var tempArray = array;
            var count = tempArray.Length;

            Array.Sort(tempArray);

            decimal medianValue = 0;

            if (count % 2 == 0)
            {
                // count is even, need to get the middle two elements, add them together, then divide by 2
                var middleElement1 = tempArray[(count / 2) - 1];
                var middleElement2 = tempArray[(count / 2)];
                medianValue = (middleElement1 + middleElement2) / 2;
            }
            else
            {
                // count is odd, simply get the middle element.
                medianValue = tempArray[(count / 2)];
            }

            return medianValue;
        }
    }

}
