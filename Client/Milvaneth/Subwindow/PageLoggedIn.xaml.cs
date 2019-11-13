using System;
using System.Net.Http;
using System.Windows.Controls;
using HandyControl.Controls;
using Milvaneth.Common;
using Milvaneth.Common.Communication.Data;
using Milvaneth.Communication.Procedure;
using Milvaneth.Communication.Procedure.Auth;
using Milvaneth.Communication.Vendor;

namespace Milvaneth.Subwindow
{
    /// <summary>
    /// Interaction logic for PageLoggedIn.xaml
    /// </summary>
    public partial class PageLoggedIn : Page
    {
        private SubwindowRouter _sr;
        public PageLoggedIn(SubwindowRouter sr)
        {
            _sr = sr;
            InitializeComponent();

            if (_sr.Dispname == " ")
            {
                _sr.InteractiveTask(() =>
                {
                    int ret;
                    string displayName = null;

                    var local = new ProfileProcedure();

                    try
                    {
                        ret = local.Step2(out var status);

                        if (!CheckVendor.NotValidResponseCode(ret) && status != null)
                        {
                            displayName = status.DisplayName;
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        ret = 02_0000 + (int)(ex.Data["StatusCode"]);
                    }
                    catch (Exception)
                    {
                        ret = 02_0000;
                    }

                    if (CheckVendor.NotValidResponseCode(ret))
                    {
                        Growl.Error(MessageVendor.FormatError(ret));
                    }
                    else
                    {
                        _sr.Dispname = displayName;

                        if (this.Dispatcher != null && !this.Dispatcher.CheckAccess())
                        {
                            this.Dispatcher.Invoke(() => NameDisplay.Text = !string.IsNullOrWhiteSpace(_sr.Dispname)
                                ? $"已登录 {_sr.Dispname}"
                                : $"已登录 {_sr.Username}");
                        }
                        else
                        {
                            NameDisplay.Text = !string.IsNullOrWhiteSpace(_sr.Dispname)
                                ? $"已登录 {_sr.Dispname}"
                                : $"已登录 {_sr.Username}";
                        }
                    }
                });
            }

            NameDisplay.Text = !string.IsNullOrWhiteSpace(_sr.Dispname) ? $"已登录 {_sr.Dispname}" : $"已登录 {_sr.Username}";
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _sr.InteractiveTask(() =>
            {
                int ret;

                _sr.Procedure = null;
                var local = new LogoutProcedure();

                try
                {
                    ret = local.Step1();
                }
                catch (HttpRequestException ex)
                {
                    ret = 02_0000 + (int)(ex.Data["StatusCode"]);
                }
                catch (Exception)
                {
                    ret = 02_0000;
                }

                if (CheckVendor.NotValidResponseCode(ret))
                {
                    Growl.Error(MessageVendor.FormatError(ret));
                }
                else
                {
                    ApiVendor.SetRenew(null);
                    SubwindowNavigator.Navigate(SubwindowPage.LoggedOut);
                }
            });
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.LoggedOut));
        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.AccountInfo));
        }

        private void Button_Click_2(object sender, System.Windows.RoutedEventArgs e)
        {
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.Change1));
        }
    }
}
