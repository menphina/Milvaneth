using Milvaneth.Common;
using Milvaneth.Common.Communication.Data;
using Milvaneth.Communication.Data;
using Milvaneth.Communication.Procedure;
using Milvaneth.Communication.Vendor;
using Milvaneth.Definitions.Communication.Data;
using Milvaneth.Interactive;
using Milvaneth.Overlay;
using Milvaneth.Service;
using Milvaneth.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using Application = System.Windows.Application;

namespace Milvaneth
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private NotifyIcon SystemTrayIcon;
        private ContextMenuStrip SystemTrayMenu;
        private ToolStripMenuItem SystemTrayMenuShow;
        private ToolStripMenuItem SystemTrayMenuOverlay;
        private ToolStripMenuItem SystemTrayMenuExit;
        private ToolStripSeparator SystemTrayMenuSep1;
        private ToolStripSeparator SystemTrayMenuSep2;

        private string _tabId;
        private Mutex _mutex;

        private EnvironmentFile env = new EnvironmentFile();
        private BindingRouter br;
        private ItemOverviewPage ip;
        private SettingPage sp;
        private LogPage lp;
        private AboutPage ap;
        private OverlayBase ov;
        private DataPool pool;

        public MainWindow(Mutex mutex, out bool showUsrDiag)
        {
            // data router
            _mutex = mutex;
            br = new BindingRouter(env);
            
            // services
            if(!ApiVendor.TryPrepareRestApi())
                InlineLogic.ServerUnreachableLogic();

            showUsrDiag = SupportStatic.RemoteInitialize(br.ConfAutoUpdate);
            SupportStatic.InitializeAll();

            pool = new DataPool(x => LoggingManagementService.WriteLine(x, "UplMgmt"));

            SubprocessManagementService.OnOverrideRequested += br.SaveConfig;
            LoggingManagementService.OnNewLogLine += br.DummyListener;
            TransmittingManagementService.OnDataOutput += pool.SinkData;
            TransmittingManagementService.OnDataOutput += LocalHandler;

            // user interface
            InitializeComponent();
            InitializeWinformComponent();
            CommandBindings.Add(new CommandBinding(BindingRouter.UiSearchLineEnterCommand, OnCommandLineEnter));
            this.DataContext = br;

#if DEBUG
            Title = "Milvaneth Prélude";
#endif

            // subviews
            ip = new ItemOverviewPage(br);
            sp = new SettingPage(br);
            lp = new LogPage(br);
            ap = new AboutPage(br);

            // custom message
            InlineLogic.GlobalMessageLogic();

            if (!showUsrDiag)
                ShowOverlay();

            // finish initialize
            if (br.ConfAutoAttach)
                SubprocessManagementService.SpawnAll(out _);
            if (!br.ConfNotUseHunter)
                SubprocessManagementService.SpawnHunter();

            br.UiActiveTab = 3;
            MainFrame.Navigate(ap);
        }

        public void ShowOverlay()
        {
            // overlay
            ReloadOverlay();
            br.OnOverlayReloading += ReloadOverlay;
        }

        private void InitializeWinformComponent()
        {
            var iconStream = Application
                .GetResourceStream(new Uri("pack://application:,,,/Milvaneth;component/milvaneth_small.ico")).Stream;

            SystemTrayIcon = new NotifyIcon();
            SystemTrayMenu = new ContextMenuStrip();
            SystemTrayMenuShow = new ToolStripMenuItem();
            SystemTrayMenuOverlay = new ToolStripMenuItem();
            SystemTrayMenuExit = new ToolStripMenuItem();
            SystemTrayMenuSep1 = new ToolStripSeparator();
            SystemTrayMenuSep2 = new ToolStripSeparator();

            //
            // SystemTrayIcon
            //
            SystemTrayIcon.Icon = new System.Drawing.Icon(iconStream);
            SystemTrayIcon.Text = "Milvaneth 客户端";
            SystemTrayIcon.ContextMenuStrip = SystemTrayMenu;
            SystemTrayIcon.Visible = true;
            SystemTrayIcon.MouseDoubleClick += Tray_Click;
            SystemTrayMenu.SuspendLayout();
            // 
            // SystemTrayMenu
            // 
            SystemTrayMenu.Items.AddRange(new ToolStripItem[]
            {
                SystemTrayMenuShow,
                SystemTrayMenuSep1,
                SystemTrayMenuOverlay,
                SystemTrayMenuSep2,
                SystemTrayMenuExit
            });
            SystemTrayMenu.Name = "SystemTrayMenu";
            SystemTrayMenu.Size = new System.Drawing.Size(147, 82);
            // 
            // SystemTrayMenuShow
            // 
            SystemTrayMenuShow.Name = "SystemTrayMenuShow";
            SystemTrayMenuShow.Size = new System.Drawing.Size(146, 22);
            SystemTrayMenuShow.Text = "显示主界面";
            SystemTrayMenuShow.Click += Tray_Click;
            // 
            // SystemTrayMenuSep1
            // 
            SystemTrayMenuSep1.Name = "SystemTrayMenuSep2";
            SystemTrayMenuSep1.Size = new System.Drawing.Size(143, 6);
            // 
            // SystemTrayMenuOverlay
            // 
            SystemTrayMenuOverlay.Name = "SystemTrayMenuOverlay";
            SystemTrayMenuOverlay.Size = new System.Drawing.Size(146, 22);
            SystemTrayMenuOverlay.Text = "开启/关闭悬浮窗";
            SystemTrayMenuOverlay.Click += Overlay_Click;
            // 
            // SystemTrayMenuSep2
            // 
            SystemTrayMenuSep2.Name = "SystemTrayMenuSep2";
            SystemTrayMenuSep2.Size = new System.Drawing.Size(143, 6);
            // 
            // SystemTrayMenuExit
            // 
            SystemTrayMenuExit.Name = "SystemTrayMenuExit";
            SystemTrayMenuExit.Size = new System.Drawing.Size(146, 22);
            SystemTrayMenuExit.Text = "退出 Milvaneth";
            SystemTrayMenuExit.Click += Exit_Click;
            SystemTrayMenu.ResumeLayout(false);
        }

        private void LocalHandler(int gameId, PackedResult result)
        {
            var worldId = pool.GetContext(gameId).World;

            if (result.Type == PackedResultType.MarketRequest)
            {
                var itemid = ((MarketRequestResult) result.Result).ItemId;
                Task.Run(() =>
                {
                    RETRY:
                    int ret = 0;
                    var listing = new List<ListingData>();
                    var history = new List<HistoryData>();
                    PackedResultBundle res = null;

                    try
                    {
                        var local = new ExchangeProcedure();

                        try
                        {
                            ret = local.Step2(itemid, out res);
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

                        var lst = (List<ListingData>)DataHolder.GetCache(worldId, itemid, 0);
                        var hst = (List<HistoryData>)DataHolder.GetCache(worldId, itemid, 1);

                        if (lst != null)
                        {
                            listing.RemoveAll(x => x.World == worldId);
                            br.OverlayListingData = listing.Concat(lst)
                                .OrderBy(x => x.UnitPrice).ToList();
                        }
                        else
                        {
                            lst = br.OverlayListingData.Where(x => x.World == worldId && x.ItemId == itemid).ToList();

                            if (lst.Any())
                            {
                                listing.RemoveAll(x => x.World == worldId);
                                br.OverlayListingData = listing.Concat(lst)
                                    .OrderBy(x => x.UnitPrice).ToList();
                            }

                            br.OverlayListingData = listing.OrderBy(x => x.UnitPrice).ToList();
                        }

                        if (hst != null)
                        {
                            history.RemoveAll(x => x.World == worldId);
                            br.OverlayHistoryData = history.Concat(hst)
                                .OrderByDescending(x => x.PurchaseTime).ToList();
                        }
                        else
                        {
                            hst = br.OverlayHistoryData.Where(x => x.World == worldId && x.ItemId == itemid).ToList();

                            if (hst.Any())
                            {
                                history.RemoveAll(x => x.World == worldId);
                                br.OverlayHistoryData = history.Concat(hst)
                                    .OrderByDescending(x => x.PurchaseTime).ToList();
                            }

                            br.OverlayHistoryData = history.OrderByDescending(x => x.PurchaseTime).ToList();
                        }
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
                            $"Api Error: {MessageVendor.FormatError(ret)} on requesting {DictionaryManagementService.Item[itemid]}",
                            "ApiSys");
                    }
                });
            }

            if (result.Type == PackedResultType.MarketHistory)
            {
                var r = (MarketHistoryResult) result.Result;
                br.OverlayItemId = r.ItemId;
                var t = new List<HistoryData>();
                foreach (var i in r.HistoryItems)
                {
                    t.Add(new HistoryData
                    {
                        BuyerName = i.BuyerName,
                        IsHq = i.IsHq == 1,
                        ItemId = i.ItemId,
                        OnMannequin = i.OnMannequin == 1,
                        PurchaseTime = Helper.UnixTimeStampToDateTime(i.PurchaseTime),
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        UpdateTime = DateTime.Now,
                        World = worldId,
                        Zone = 0
                    });
                }

                DataHolder.AddCache(worldId, r.ItemId, 1, t, 120);
            }

            if (result.Type == PackedResultType.MarketListing)
            {
                var r = (MarketListingResult) result.Result;
                var t = new List<ListingData>();
                foreach (var i in r.ListingItems)
                {
                    t.Add(new ListingData
                    {
                        ItemId = i.ItemId,
                        IsHq = i.IsHq == 1,
                        Materia = new int[] {i.Materia1, i.Materia2, i.Materia3, i.Materia4, i.Materia5},
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity,
                        Tax = i.TotalTax,
                        Retainer = i.RetainerName,
                        Artisan = i.ArtisanId.ToString("X"),
                        OnMannequin = i.OnMannequin == 1,
                        PlayerName = i.PlayerName,
                        RetainerLocation = i.RetainerLocation,
                        DyeId = i.DyeId,
                        ListingTime = Helper.UnixTimeStampToDateTime(i.UpdateTime),
                        UpdateTime = DateTime.Now,
                        World = worldId,
                        Zone = 0,
                    });
                }

                var sample = t.FirstOrDefault();
                if (sample != null)
                {
                    DataHolder.AddCache(worldId, sample.ItemId, 0, t, 120);
                }
            }

            LoggingManagementService.WriteLine($"Data Received: {result.Type.ToString()} @ {DictionaryManagementService.World[worldId]}", $"Game {gameId}");
        }

        #region Events

        protected override void OnSourceInitialized(EventArgs e)
        {
            var hwndSource = (HwndSource)PresentationSource.FromVisual(this);
            hwndSource.AddHook(WndProcHook);
            base.OnSourceInitialized(e);
        }

        private IntPtr WndProcHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handeled)
        {
            if (msg != NativeMethods.WM_SHOWME) return IntPtr.Zero;

            ShowMe();
            handeled = true;
            return IntPtr.Zero;
        }

        private void ShowMe()
        {
            if (this.WindowState == WindowState.Minimized || this.Visibility == Visibility.Collapsed || this.Visibility == Visibility.Hidden)
            {
                this.WindowState = WindowState.Normal;
                this.Show();
            }

            this.Topmost = true;
            this.Topmost = false;
        }

        private void GeneralTab_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is DockPanel dockPanel)) return;

            _tabId = dockPanel.Name;
        }

        private void GeneralTab_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is DockPanel dockPanel)) return;

            if (dockPanel.Name != _tabId) return;

            switch (_tabId)
            {
                case nameof(ItemTab):
                    br.UiActiveTab = 0;
                    MainFrame.Navigate(ip);
                    break;
                case nameof(SettingTab):
                    br.UiActiveTab = 1;
                    MainFrame.Navigate(sp);
                    break;
                case nameof(LogsTab):
                    br.UiActiveTab = 2;
                    MainFrame.Navigate(lp);
                    br.OnPropertyChanged(nameof(BindingRouter.LogLines), true); // trigger at last second to reduce performance impact, will has lag, but not too much
                    break;
                case nameof(AboutTab):
                    br.UiActiveTab = 3;
                    MainFrame.Navigate(ap);
                    break;
                default:
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            br.UiSearchLine = "";
        }

        public void OnCommandLineEnter(object sender, ExecutedRoutedEventArgs e)
        {
            br.UiActiveTab = 0;
            MainFrame.Navigate(ip);
            var dic = DictionaryManagementService.SearchItem(br.UiSearchLine);
            br.OverlayOverviewData =
                dic.Select(x => new OverviewData {ItemId = x.Key, UpdateTime = DateTime.MinValue}).ToList();

            Task.Run(() =>
            {
                RETRY:

                int ret = 0;

                try
                {
                    var local = new ExchangeProcedure();
                    var partCnt = 0;
                    OverviewResponse res = null;

                    do
                    {
                        try
                        {
                            ret = local.Step2(dic.Keys.ToArray(), partCnt++, out res);
                        }
                        catch (HttpRequestException exception)
                        {
                            ret = 02_0000 + (int) exception.Data["StatusCode"];
                        }
                        

                        if (!CheckVendor.NotValidResponseCode(ret))
                        {
                            var overviewDic = br.OverlayOverviewData.ToDictionary(x => x.ItemId, x => x);

                            foreach (var item in res.Results)
                            {
                                overviewDic[item.ItemId] =
                                    OverviewData.FromResultItem(overviewDic[item.ItemId], item);
                            }

                            br.OverlayOverviewData = overviewDic.Values.ToList();
                        }
                    } while (!CheckVendor.NotValidResponseCode(ret) && !res.FinalPart);

                    if (ret % 10000 == 0511 && ApiVendor.ValidateAndRenewToken())
                    {
                        goto RETRY;
                    }
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
                        $"Api Error: {MessageVendor.FormatError(ret)} on searching {br.UiSearchLine}", "ApiSys");
                }
            });
        }

        private void Tray_Click(object sender, EventArgs e)
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void Overlay_Click(object sender, EventArgs e)
        {
            br.ConfShowOverlay = !br.ConfShowOverlay;
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            this.Hide();
            base.OnStateChanged(e);

            DoGracefulExit();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized && br.ConfMinimizeToTray)
                this.Hide();

            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;

            this.Hide();

            if (br.ConfFullExit)
                DoGracefulExit();

            base.OnClosing(e);
        }

        #endregion

        private void ReloadOverlay()
        {
            ov?.Close();
            if (!OverlayLoader.TryLoadOverlay(br.InternalOverlayAssemblyPath, br, out ov))
            {
                br.InternalOverlayAssemblyPath = null;
            }

            br.ClearOverlayDelegates();
            br.OnOverlayVisibilityChanged += ov.SetVisibility;
            br.OnOverlayClickthroughChanged += ov.SetClickthrough;
            br.ConfShowOverlay = br.ConfShowOverlay; // take effect once
            br.ConfClickthough = br.ConfClickthough;
        }

        private void DoGracefulExit()
        {
            _mutex.ReleaseMutex();
            _mutex.Close();
            this.Hide();
            ov?.Close();
            br.SaveConfig();
            SystemTrayIcon.Visible = false;

            SupportStatic.ExitAll();

            Environment.Exit(0);
        }
    }
}
