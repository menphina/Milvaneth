using Milvaneth.Common.Communication.Data;
using Milvaneth.Communication.Procedure;
using Milvaneth.Communication.Vendor;
using Milvaneth.Overlay;
using Milvaneth.Service;
using Milvaneth.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Milvaneth
{
    /// <summary>
    /// Interaction logic for ItemOverviewPage.xaml
    /// </summary>
    public partial class ItemOverviewPage : Page
    {
        private readonly BindingRouter br;
        private readonly OverviewPresenterViewModel opvm;
        public ItemOverviewPage(BindingRouter dataContext)
        {
            opvm = new OverviewPresenterViewModel();
            
            InitializeComponent();
            DataContext = dataContext;
            this.ElementView.DataContext = opvm;
            br = dataContext;

            dataContext.PropertyChanged += OnPropertyChanged;
        }

        protected void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null) return;

            switch (e.PropertyName)
            {
                case nameof(br.OverlayOverviewData):
                    opvm.UpdateData(br.OverlayOverviewData);
                    break;

                default:
                    break;
            }
        }

        private void ElementView_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ElementView.SelectedItem as OverviewPresenter;
            if (item != null)
            {
                br.OverlayItemId = item.ItemId;

                Task.Run(() =>
                {
                    RETRY:
                    int ret = 0;
                    br.OverlayListingData.Clear();
                    br.OverlayHistoryData.Clear();
                    var listing = new List<ListingData>();
                    var history = new List<HistoryData>();
                    PackedResultBundle res = null;

                    try
                    {
                        var local = new ExchangeProcedure();

                        try
                        {
                            ret = local.Step2(item.ItemId, out res);
                        }
                        catch (HttpRequestException exception)
                        {
                            ret = 02_0000 + (int)exception.Data["StatusCode"];
                        }


                        if (!CheckVendor.NotValidResponseCode(ret))
                        {
                            listing.AddRange(res.Listings.Select(x =>
                                ListingData.FromResultItem(x.RawItem, x.ReportTime, 0, x.WorldId)));
                            history.AddRange(res.Histories.Select(x =>
                                HistoryData.FromResultItem(x.RawItem, x.ReportTime, 0, x.WorldId)));
                        }

                        if (ret % 10000 == 0511 && ApiVendor.ValidateAndRenewToken())
                        {
                            goto RETRY;
                        }

                        br.OverlayListingData = listing.OrderBy(x => x.UnitPrice).ToList();
                        br.OverlayHistoryData = history.OrderByDescending(x => x.PurchaseTime).ToList();
                    }
                    catch (HttpRequestException ex)
                    {
                        ret = 02_0000 + (int)(ex.Data["StatusCode"]);
                        if (ret == 02_0511 && ApiVendor.ValidateAndRenewToken())
                        {
                            goto RETRY;
                        }
                    }
                    catch (Exception)
                    {
                        ret = 02_0000;
                    }

                    if (CheckVendor.NotValidResponseCode(ret))
                    {
                        LoggingManagementService.WriteLine(
                            $"Api Error: {MessageVendor.FormatError(ret)} on requesting {DictionaryManagementService.Item[item.ItemId]}",
                            "ApiSys");
                    }
                });
            }
        }
    }
}
