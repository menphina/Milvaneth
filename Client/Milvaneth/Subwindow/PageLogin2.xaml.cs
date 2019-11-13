using HandyControl.Controls;
using Milvaneth.Communication.Procedure;
using Milvaneth.Communication.Vendor;
using System;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Milvaneth.Subwindow
{
    /// <summary>
    /// Interaction logic for PageLogin2.xaml
    /// </summary>
    public partial class PageLogin2 : Page
    {
        private SubwindowRouter _sr;
        public PageLogin2(SubwindowRouter sr)
        {
            _sr = sr;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.LoggedOut));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.Login1));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var password = Password.Password.ToCharArray();

            _sr.InteractiveTask(() =>
            {
                int ret = 00_0000;

                try
                {
                    if (!(_sr.Procedure is LoginProcedure local))
                    {
                        ret = 02_0009;
                        goto FAIL;
                    }

                    ret = CheckVendor.NotValidResponseCode(ret) ? ret : local.Step2(Encoding.UTF8.GetBytes(password));

                    byte[] token = null;

                    ret = CheckVendor.NotValidResponseCode(ret) ? ret : local.Step3(out token);

                    _sr.RenewToken = token;

                    if (!CheckVendor.NotValidResponseCode(ret))
                    {
                        var prof = new ProfileProcedure();
                        ret = prof.Step2(out var status);
                        _sr.Username = status.Username;
                        _sr.Dispname = status.DisplayName;
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

                FAIL:
                if (CheckVendor.NotValidResponseCode(ret))
                {
                    Growl.Error(MessageVendor.FormatError(ret));
                }
                else
                {
                    _sr.Procedure = null;
                    SubwindowNavigator.Navigate(SubwindowPage.LoggedIn);
                }
            });
        }
    }
}
