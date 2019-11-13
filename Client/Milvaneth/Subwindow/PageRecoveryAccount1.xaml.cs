using HandyControl.Controls;
using Milvaneth.Common;
using Milvaneth.Communication.Procedure;
using Milvaneth.Communication.Vendor;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace Milvaneth.Subwindow
{
    /// <summary>
    /// Interaction logic for PageRecoveryAccount1.xaml
    /// </summary>
    public partial class PageRecoveryAccount1 : Page
    {
        private SubwindowRouter _sr;
        public PageRecoveryAccount1(SubwindowRouter sr)
        {
            _sr = sr;
            InitializeComponent();

            _sr.InteractiveTask(() =>
            {
                int ret;

                if (!SubwindowDataCollector.Collect(5 * 60 * 1000, out var service, out var character))
                {
                    Growl.Error("等待超时，请返回上一页重试");
                }

                _sr.Procedure = null;
                var local = new RecoveryProcedure();

                try
                {
                    if (string.IsNullOrEmpty(_sr.Username))
                    {
                        ret = 02_0009;
                        goto FAIL;
                    }

                    ret = local.Step1(_sr.Username, service, character, Helper.GetTrace());
                }
                catch (HttpRequestException ex)
                {
                    ret = 02_0000 + (int)(ex.Data["StatusCode"]);
                }
                catch (Exception)
                {
                    ret = 02_0000;
                }

                FAIL:
                if (CheckVendor.NotValidResponseCode(ret))
                {
                    Growl.Error(MessageVendor.FormatError(ret));
                }
                else
                {
                    _sr.Procedure = local;
                    SubwindowNavigator.Navigate(SubwindowPage.Recovery2);
                }
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.LoggedOut));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.RecoveryEntry));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.Recovery2));
        }
    }
}
