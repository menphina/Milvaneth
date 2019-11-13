using HandyControl.Controls;
using Milvaneth.Common;
using Milvaneth.Common.Communication.Data;
using Milvaneth.Communication.Procedure;
using Milvaneth.Communication.Vendor;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;

namespace Milvaneth.Subwindow
{
    /// <summary>
    /// Interaction logic for PageAccountInfo.xaml
    /// </summary>
    public partial class PageAccountInfo : Page
    {
        private SubwindowRouter _sr;
        public PageAccountInfo(SubwindowRouter sr)
        {
            _sr = sr;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var displayName = DisplayName.Text;

            _sr.InteractiveTask(() =>
            {
                int ret;

                _sr.Procedure = null;
                var local = new ProfileProcedure();
                
                try
                {
                    ret = local.Step1(displayName, Helper.GetTrace(), null);

                    AccountStatus status = null;

                    ret = CheckVendor.NotValidResponseCode(ret) ? ret : local.Step2(out status);

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
                    Growl.Success("成功修改显示名称");
                    _sr.Dispname = displayName;
                    SubwindowNavigator.Navigate(SubwindowPage.LoggedIn);
                }
            });
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _sr.InteractiveTask(() =>
            {
                SubwindowNavigator.Navigate(SubwindowPage.LoggedIn);
            });
        }
    }
}
