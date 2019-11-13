using HandyControl.Controls;
using Milvaneth.Common;
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
    /// Interaction logic for PageChange1.xaml
    /// </summary>
    public partial class PageChange1 : Page
    {
        private SubwindowRouter _sr;
        public PageChange1(SubwindowRouter sr)
        {
            _sr = sr;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.LoggedIn));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var password = OldPassword.Password.ToCharArray();
            _sr.InteractiveTask(() =>
            {
                int ret;

                _sr.Procedure = null;
                var local = new ChangeProcedure();

                try
                {
                    if (string.IsNullOrEmpty(_sr.Username))
                    {
                        ret = 02_0009;
                        goto FAIL;
                    }

                    ret = local.Step1(_sr.Username, Helper.GetTrace());

                    ret = CheckVendor.NotValidResponseCode(ret) ? ret : local.Step2(Encoding.UTF8.GetBytes(password));

                    ret = CheckVendor.NotValidResponseCode(ret) ? ret : local.Step3();
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
                    SubwindowNavigator.Navigate(SubwindowPage.Change2);
                }
            });
        }
    }
}
