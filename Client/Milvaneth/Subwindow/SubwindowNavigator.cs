using Milvaneth.Common;
using Milvaneth.Communication.Vendor;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Milvaneth.Subwindow
{
    internal static class SubwindowNavigator
    {
        private static SubwindowPage _page;
        private static SubwindowRouter _router;
        private static Frame _naviFrame;
        private static Dispatcher _dispatcher;

        public static void Navigate(SubwindowPage page)
        {
            _page = page;
            Switch(page);
        }

        public static SubwindowPage GetCurrent()
        {
            return _page;
        }

        public static void PrepareDefaultPage(SubwindowRouter router, Frame frame, Dispatcher dispatcher)
        {
            _router = router;
            _naviFrame = frame;
            _dispatcher = dispatcher;

            if (File.Exists(Helper.GetMilFilePathRaw(".consent")))
            {
                Navigate(ApiVendor.HasToken() ? SubwindowPage.LoggedIn : SubwindowPage.LoggedOut);
                return;
            }

            Navigate(SubwindowPage.Welcome);
        }

        private static void Switch(SubwindowPage page)
        {
            if (!_dispatcher.CheckAccess())
            {
                _dispatcher.Invoke(() => SwitchInternal(page));
            }
            else
            {
                SwitchInternal(page);
            }
        }

        private static void SwitchInternal(SubwindowPage page)
        {
            switch (page)
            {
                case SubwindowPage.Welcome:
                    _naviFrame.Navigate(new PageWelcome(_router));
                    break;
                case SubwindowPage.Consent:
                    _naviFrame.Navigate(new PageConsent(_router));
                    break;
                case SubwindowPage.LoggedIn:
                    _naviFrame.Navigate(new PageLoggedIn(_router));
                    break;
                case SubwindowPage.LoggedOut:
                    _naviFrame.Navigate(new PageLoggedOut(_router));
                    break;
                case SubwindowPage.AccountInfo:
                    _naviFrame.Navigate(new PageAccountInfo(_router));
                    break;
                case SubwindowPage.Login1:
                    _naviFrame.Navigate(new PageLogin1(_router));
                    break;
                case SubwindowPage.Login2:
                    _naviFrame.Navigate(new PageLogin2(_router));
                    break;
                case SubwindowPage.Change1:
                    _naviFrame.Navigate(new PageChange1(_router));
                    break;
                case SubwindowPage.Change2:
                    _naviFrame.Navigate(new PageChange2(_router));
                    break;
                case SubwindowPage.Register1:
                    _naviFrame.Navigate(new PageRegister1(_router));
                    break;
                case SubwindowPage.Register2:
                    _naviFrame.Navigate(new PageRegister2(_router));
                    break;
                case SubwindowPage.RecoveryEntry:
                    _naviFrame.Navigate(new PageRecoveryEntry(_router));
                    break;
                case SubwindowPage.Recovery1:
                    _naviFrame.Navigate(new PageRecovery1(_router));
                    break;
                case SubwindowPage.RecoveryAccount1:
                    _naviFrame.Navigate(new PageRecoveryAccount1(_router));
                    break;
                case SubwindowPage.RecoveryEmail1:
                    _naviFrame.Navigate(new PageRecoveryEmail1(_router));
                    break;
                case SubwindowPage.RecoveryEmail2:
                    _naviFrame.Navigate(new PageRecoveryEmail2(_router));
                    break;
                case SubwindowPage.Recovery2:
                    _naviFrame.Navigate(new PageRecovery2(_router));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(page), page, null);
            }
        }
    }

    internal enum SubwindowPage
    {
        Welcome,
        Consent,
        LoggedIn,
        LoggedOut,
        AccountInfo,
        Login1,
        Login2,
        Change1,
        Change2,
        Register1,
        Register2,
        Recovery1,
        RecoveryEntry,
        RecoveryAccount1,
        RecoveryEmail1,
        RecoveryEmail2,
        Recovery2,
    }
}
