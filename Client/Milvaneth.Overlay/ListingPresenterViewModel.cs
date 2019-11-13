using Milvaneth.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Milvaneth.Overlay
{
    public class ListingPresenterViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ObservableCollection<ListingPresenter> _bindingCollection;
        public ObservableCollection<ListingPresenter> BindingCollection
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
#pragma warning disable CS0618
            var id = 2714;
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
                    RetainerLocation = 1,
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
#pragma warning restore CS0618
            UpdateData(id, ld);
        }

        public void UpdateData(int itemId, List<ListingData> ld)
        {
            var t = new ObservableCollection<ListingPresenter>();

            if (ld == null) return;

            foreach (var i in ld)
            {
                if(i.ItemId != itemId) continue;

                try
                {
                    t.Add(new ListingPresenter(i.IsHq, i.Materia, i.UnitPrice, i.Quantity, i.World,
                        i.Retainer, i.RetainerLocation, i.UpdateTime));
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

    public class ListingPresenter
    {
        public ListingPresenter(bool hq, int[] materia, int unit, int quan, int server, string retainer, int loc,
            DateTime update)
        {
            HqMark = hq ? "⭐" : "";
            Materia = FromMateriaIds(materia);
            MateriaCount = materia.Length;
            UnitPrice = unit;
            Quantity = quan;
            TotalPrice = quan * unit;
            Tax = quan * unit * 5 / 100;
            TotalPriceAfterTax = TotalPrice + Tax;
            Server = DictionaryManagementService.LocalizedWorld.TryGetValue(server, out var name) ? name : DictionaryManagementService.World[server];
            Retainer = $"[{DictionaryManagementService.RetainerAbbr[loc]}]{retainer}";
            UpdateTime = $"{(int)Math.Min((DateTime.Now - update).TotalDays, 999)} 天前";
        }

        public string HqMark { get; set; }
        public string Materia { get; set; }
        public int MateriaCount { get; set; }
        public int UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int TotalPrice { get; set; }
        public int Tax { get; set; }
        public int TotalPriceAfterTax { get; set; }
        public string Server { get; set; }
        public string Retainer { get; set; }
        public string UpdateTime { get; set; }

        private string FromMateriaIds(int[] ids)
        {
            if (ids == null) return "";
            return ids.Aggregate("", (current, i) => current + (DictionaryManagementService.MateriaAbbr.TryGetValue(i, out var name) ? name : ""));
        }
    }
}
