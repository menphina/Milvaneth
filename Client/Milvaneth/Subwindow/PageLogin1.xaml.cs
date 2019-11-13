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
    /// Interaction logic for PageLogin1.xaml
    /// </summary>
    public partial class PageLogin1 : Page
    {
        private SubwindowRouter _sr;
        public PageLogin1(SubwindowRouter sr)
        {
            _sr = sr;
            InitializeComponent();

            if (!string.IsNullOrEmpty(_sr.Username))
            {
                Username.Text = _sr.Username;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.LoggedOut));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var username = Username.Text;

            _sr.InteractiveTask(() =>
            {
                int ret;

                _sr.Procedure = null;
                var local = new LoginProcedure();

                try
                {
                    ret = local.Step1(username, Helper.GetTrace());
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
                    _sr.Procedure = local;
                    SubwindowNavigator.Navigate(SubwindowPage.Login2);
                }
            });
        }
    }
}
