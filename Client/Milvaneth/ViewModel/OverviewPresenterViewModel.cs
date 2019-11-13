using Milvaneth.Overlay;
using Milvaneth.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace Milvaneth.ViewModel
{
    public class OverviewPresenterViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<OverviewPresenter> _bindingCollection;
        public ObservableCollection<OverviewPresenter> BindingCollection
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
            var od = new List<OverviewData>
            {
                new OverviewData
                {
                    Demand = 12,
                    ItemId = 1,
                    OpenListing = 22,
                    UpdateTime = DateTime.Now,
                },
                new OverviewData
                {
                    Demand = 999,
                    ItemId = 2714,
                    OpenListing = 999,
                    UpdateTime = DateTime.Now.AddDays(-1),
                },
                new OverviewData
                {
                    Demand = 12,
                    ItemId = 2210,
                    OpenListing = 22,
                    UpdateTime = DateTime.Now.AddDays(-998),
                },
                new OverviewData
                {
                    Demand = 12,
                    ItemId = 26173,
                    OpenListing = 22,
                    UpdateTime = DateTime.Now.AddDays(-1000),
                },
            };
            UpdateData(od);
        }

        public void UpdateData(List<OverviewData> od)
        {
            var t = new ObservableCollection<OverviewPresenter>();

            if (od == null) return;

            foreach (var i in od)
            {
                try
                {
                    t.Add(new OverviewPresenter(i.ItemId, 0, i.OpenListing, 0, i.Demand, i.UpdateTime));
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

    public class OverviewPresenter
    {
        public OverviewPresenter(int itemId, int price, int openListing, int sale, int demand, DateTime update)
        {
            ItemId = itemId;
            ItemIcon = IconManagementService.GetIcon(itemId, false);
            ItemName = DictionaryManagementService.GetName(itemId);
            MinPrice = price;
            OpenListing = openListing;
            WeekSale = sale;
            Demand = demand;
            UpdateTime = $"{(int)Math.Min((DateTime.Now - update).TotalDays, 999)} 天前";
        }

        public int ItemId { get; set; }
        public BitmapSource ItemIcon { get; set; }
        public string ItemName { get; set; }
        public int MinPrice { get; set; }
        public int OpenListing { get; set; }
        public int WeekSale { get; set; }
        public int Demand { get; set; }
        public string UpdateTime { get; set; }
    }
}
