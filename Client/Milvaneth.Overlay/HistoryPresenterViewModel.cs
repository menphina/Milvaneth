using Milvaneth.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Milvaneth.Overlay
{
    public class HistoryPresenterViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<HistoryPresenter> _bindingCollection;
        public ObservableCollection<HistoryPresenter> BindingCollection
        {
            get => _bindingCollection;
            set
            {
                _bindingCollection = value;
                OnPropertyChanged(nameof(BindingCollection));
            }
        }

        internal void LoadTestData()
        {
            var id = 2714;
            var hd = new List<HistoryData>
            {
                new HistoryData
                {
                    BuyerName = "玩家1",
                    IsHq = true,
                    ItemId = 2714,
                    OnMannequin = false,
                    PurchaseTime = DateTime.Now,
                    Quantity = 12,
                    UnitPrice = 1234,
                    UpdateTime = DateTime.Now,
                    World = 1106,
                    Zone = 1,
                },
                new HistoryData
                {
                    BuyerName = "名字超长的玩家",
                    IsHq = true,
                    ItemId = 2714,
                    OnMannequin = false,
                    PurchaseTime = DateTime.Now.AddDays(-3),
                    Quantity = 12,
                    UnitPrice = 1234,
                    UpdateTime = DateTime.Now.AddDays(-998),
                    World = 1107,
                    Zone = 1,
                },
                new HistoryData
                {
                    BuyerName = "玩家2",
                    IsHq = true,
                    ItemId = 2714,
                    OnMannequin = false,
                    PurchaseTime = DateTime.Now.AddDays(-27),
                    Quantity = 12,
                    UnitPrice = 99999999,
                    UpdateTime = DateTime.Now.AddDays(-24),
                    World = 1106,
                    Zone = 1,
                },
                new HistoryData
                {
                    BuyerName = "玩家3",
                    IsHq = true,
                    ItemId = 2714,
                    OnMannequin = false,
                    PurchaseTime = DateTime.Now.AddDays(16),
                    Quantity = 12,
                    UnitPrice = 1234,
                    UpdateTime = DateTime.Now.AddDays(3),
                    World = 1106,
                    Zone = 1,
                },
                new HistoryData
                {
                    BuyerName = "不应显示",
                    IsHq = true,
                    ItemId = 2715,
                    OnMannequin = false,
                    PurchaseTime = DateTime.Now,
                    Quantity = 12,
                    UnitPrice = 1234,
                    UpdateTime = DateTime.Now,
                    World = 1106,
                    Zone = 1,
                },
            };
            UpdateData(id, hd);
        }

        public void UpdateData(int itemId, List<HistoryData> hd)
        {
            var t = new ObservableCollection<HistoryPresenter>();

            if (hd == null) return;

            foreach (var i in hd)
            {
                if (i.ItemId != itemId) continue;

                try
                {
                    t.Add(new HistoryPresenter(i.IsHq, i.BuyerName, i.UnitPrice, i.Quantity, i.World,
                        i.PurchaseTime, i.UpdateTime));
                }
                catch
                {
                    // ignored
                }
            }

            BindingCollection = t;
        }

        internal void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class HistoryPresenter
    {
        public HistoryPresenter(bool hq, string buyer, int unit, int quan, int server, DateTime purchase, DateTime update)
        {
            HqMark = hq ? "⭐" : "";
            Buyer = buyer;
            UnitPrice = unit;
            Quantity = quan;
            TotalPrice = quan * unit;
            Server = DictionaryManagementService.LocalizedWorld.TryGetValue(server, out var name) ? name : DictionaryManagementService.World[server];
            PurchaseTime = $"{purchase:yy/MM/dd HH:mm:ss}";
            UpdateTime = $"{(int)Math.Min((DateTime.Now - update).TotalDays, 999)} 天前";
        }

        public string HqMark { get; set; }
        public string Buyer { get; set; }
        public int UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int TotalPrice { get; set; }
        public string Server { get; set; }
        public string PurchaseTime { get; set; }
        public string UpdateTime { get; set; }
    }
}
