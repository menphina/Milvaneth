using Milvaneth.Overlay;
using Milvaneth.Service;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace Milvaneth
{
    /// <summary>
    /// Interaction logic for Overlay.xaml
    /// </summary>
    public partial class OverlayWindow : OverlayBase
    {
        private readonly BindingRouter br;
        private string _tabId;
        private bool? _clickthrough;
        private bool? _visibility;

        // To write a overlay for Milvaneth Client, you must do the following things:
        // 1. Inherit `Milvaneth.Overlay.OverlayBase` (in Milvaneth.exe) and implement it. Namespace and class name is irrelevant.
        // 2. Add a constructor which takes `Milvaneth.Service.BindingRouter` as the only parameter
        // 3. Handle data coming from `Milvaneth.Service.BindingRouter` with "Overlay" prefix
        // Notice:
        // 1. You can read/write data without "Overlay" prefix. But please do it with caution.
        // 2. "Overlay" prefixed data also used by MainWindow. Please do not change them.
        // 3. `Milvaneth.Service` contains some static classes which may be useful.
        // 4. You should handle window style (e.g. always on top, no focus, clickthrough) by yourself. 
        // 5. You must handle `Window.Close` properly. It will be called when program exit or user switching overlay plugin.
        // 6. You should not guide user to overwrite Milvaneth.Overlay.dll. It's the global Overlay fallback.

        private ListTabPage ltp;
        private HistTabPage htp;
        private OverlayPresenterViewModel opvm;
        private ListingPresenterViewModel lpvm;
        private HistoryPresenterViewModel hpvm;
        public OverlayWindow(BindingRouter dataContext)
        {
            opvm = new OverlayPresenterViewModel();
            lpvm = new ListingPresenterViewModel();
            hpvm = new HistoryPresenterViewModel();

            ltp = new ListTabPage(lpvm);
            htp = new HistTabPage(hpvm);

            InitializeComponent();
            DataContext = dataContext;
            br = dataContext;
            dataContext.PropertyChanged += OnPropertyChanged;
            _clickthrough = null;

            this.Header.DataContext = opvm;

            opvm.UpdateData(br.OverlayItemId, br.OverlayOverviewData, br.OverlayListingData, br.OverlayHistoryData);
            lpvm.UpdateData(br.OverlayItemId, br.OverlayListingData);
            hpvm.UpdateData(br.OverlayItemId, br.OverlayHistoryData);

            MainFrame.Navigate(ltp);
        }

        public override void SetClickthrough(bool toggle)
        {
            if (_clickthrough == toggle) return;

            if(toggle)
                NativeMethods.SetClickthrough();
            else
                NativeMethods.UnsetClickthrough();

            _clickthrough = toggle;
        }

        public override void SetVisibility(bool toggle)
        {
            if (_visibility == toggle) return;

            if (toggle)
                Show();
            else
                Hide();

            _visibility = toggle;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            var hwndSource = (HwndSource)PresentationSource.FromVisual(this);
            hwndSource.AddHook(WndProcHook);
            base.OnSourceInitialized(e);
            NativeMethods.PrepareWindow(this);
        }

        protected void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null) return;

            switch (e.PropertyName)
            {
                case nameof(br.OverlayOverviewData):
                    // not related
                    break;

                case nameof(br.OverlayListingData):
                    opvm.UpdateData(br.OverlayItemId, br.OverlayOverviewData, br.OverlayListingData, br.OverlayHistoryData);
                    lpvm.UpdateData(br.OverlayItemId, br.OverlayListingData);
                    break;

                case nameof(br.OverlayHistoryData):
                    opvm.UpdateData(br.OverlayItemId, br.OverlayOverviewData, br.OverlayListingData, br.OverlayHistoryData);
                    hpvm.UpdateData(br.OverlayItemId, br.OverlayHistoryData);
                    break;

                case nameof(br.OverlayItemId):
                    opvm.UpdateData(br.OverlayItemId, br.OverlayOverviewData, br.OverlayListingData, br.OverlayHistoryData);
                    lpvm.UpdateData(br.OverlayItemId, br.OverlayListingData);
                    hpvm.UpdateData(br.OverlayItemId, br.OverlayHistoryData);
                    break;

                default:
                    break;
            }
        }

        private void GeneralTab_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is TextBlock textBlock)) return;

            _tabId = textBlock.Name;
        }

        private void GeneralTab_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is TextBlock textBlock)) return;

            if (textBlock.Name != _tabId) return;

            switch (_tabId)
            {
                case nameof(ListTab):
                    br.OverlayActiveTab = 0;
                    MainFrame.Navigate(ltp);
                    break;
                case nameof(HistTab):
                    br.OverlayActiveTab = 1;
                    MainFrame.Navigate(htp);
                    break;
                case nameof(ListGraph):
                    br.OverlayActiveTab = 2;
                    MainFrame.Navigate(ltp);
                    break;
                case nameof(HistGraph):
                    br.OverlayActiveTab = 3;
                    MainFrame.Navigate(htp);
                    break;
                default:
                    break;
            }
        }

        private IntPtr WndProcHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handeled)
        {
            if (msg != 0x0084) return IntPtr.Zero; // WM_NCHITTEST

            handeled = true;
            var x = lParam.ToInt32() & 0x0000FFFF;
            var y = (int)((lParam.ToInt32() & 0xFFFF0000) >> 16);
            var pos = new Point(x, y);
            var relativePos = MainGrid.PointFromScreen(pos);

            if (Height - relativePos.Y < 15 && Width - relativePos.X < 15)
                return (IntPtr)17; // HTBOTTOMRIGHT
            if (Height - relativePos.Y < 5)
                return (IntPtr)15; // HTBOTTOM
            if (Width - relativePos.X < 5)
                return (IntPtr)11; // HTRIGHT
            if (relativePos.Y < 50)
                return (IntPtr)2; // HTCAPTION
            return (IntPtr)1; // HTCLIENT
        }

        private static class NativeMethods
        {
            private const int GWL_EXSTYLE = -20;

            private const uint _style = 0x80020; // WS_EX_TRANSPARENT | WS_EX_LAYERED
            private const uint WS_EX_APPWINDOW = 0x00040000;
            private const uint WS_EX_TOOLWINDOW = 0x00000080;
            private const int WS_EX_NOACTIVATE = 0x08000000;

            private static IntPtr _oldStyle;

            private static IntPtr _hwnd;

            private static bool _prepared;

            [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
            private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

            [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
            private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

            [DllImport("user32.dll", SetLastError = true)]
            static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

            public static void PrepareWindow(Window window)
            {
                _hwnd = new WindowInteropHelper(window).Handle;
                _prepared = true;
                SetWindowPos(_hwnd, new IntPtr(-1), 0, 0, 0, 0, 19);

                // no focus
                var style = GetWindowLongPtr(_hwnd, GWL_EXSTYLE);
                SetWindowLongPtr(_hwnd, GWL_EXSTYLE,
                    IntPtr.Size == 8
                        ? new IntPtr((style.ToInt64() | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE) & ~WS_EX_APPWINDOW)
                        : new IntPtr((style.ToInt32() | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE) & ~WS_EX_APPWINDOW));
                _oldStyle = GetWindowLongPtr(_hwnd, GWL_EXSTYLE);
            }

            public static void SetClickthrough()
            {
                if (!_prepared) return;

                _oldStyle = GetWindowLongPtr(_hwnd, GWL_EXSTYLE);
                SetWindowLongPtr(_hwnd, GWL_EXSTYLE,
                    IntPtr.Size == 8
                        ? new IntPtr(_oldStyle.ToInt64() | _style)
                        : new IntPtr(_oldStyle.ToInt32() | _style));
            }

            public static void UnsetClickthrough()
            {
                if (!_prepared) return;

                SetWindowLongPtr(_hwnd, GWL_EXSTYLE, _oldStyle);
            }
        }
    }
}
