using HandyControl.Controls;
using Milvaneth.Common;
using Milvaneth.Communication.Procedure;
using Milvaneth.Communication.Vendor;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Milvaneth.Subwindow
{
    /// <summary>
    /// Interaction logic for PageRecoveryEmail2.xaml
    /// </summary>
    public partial class PageRecoveryEmail2 : Page
    {
        private SubwindowRouter _sr;
        public PageRecoveryEmail2(SubwindowRouter sr)
        {
            _sr = sr;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var email = _sr.Email;

            Task.Run(() =>
            {
                if (this.Dispatcher != null && !this.Dispatcher.CheckAccess())
                {
                    this.Dispatcher.Invoke(() => SendCaptcha.IsEnabled = false);
                    for (var i = 60; i > 0; i--)
                    {
                        this.Dispatcher.Invoke(() => SendCaptcha.Content = $"请稍候({i}s)");
                        Thread.Sleep(1000);
                    }
                    this.Dispatcher.Invoke(() => SendCaptcha.IsEnabled = true);
                }
                else
                {
                    SendCaptcha.IsEnabled = false;
                    for (var i = 60; i > 0; i--)
                    {
                        SendCaptcha.Content = $"请稍候({i}s)";
                        Thread.Sleep(1000);
                    }
                    SendCaptcha.IsEnabled = true;
                }
            });

            _sr.InteractiveTask(() =>
            {
                int ret;

                if (CheckVendor.NotValidEmail(email))
                {
                    Growl.Error("无效邮件地址");
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

                    ret = local.Step1(_sr.Username, email, Helper.GetTrace());
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
                }
            });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.RecoveryEmail1));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var code = VerifyCode.Text;

            _sr.InteractiveTask(() =>
            {
                int ret;

                try
                {
                    if (!(_sr.Procedure is RecoveryProcedure local))
                    {
                        ret = 02_0009;
                        goto FAIL;
                    }

                    ret = local.Step2(code);
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
                    SubwindowNavigator.Navigate(SubwindowPage.Recovery2);
                }
            });
        }
    }
}
