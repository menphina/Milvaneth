using HandyControl.Controls;
using Milvaneth.Common;
using Milvaneth.Communication.Procedure;
using Milvaneth.Communication.Vendor;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Milvaneth.Subwindow
{
    /// <summary>
    /// Interaction logic for PageRegister2.xaml
    /// </summary>
    public partial class PageRegister2 : Page
    {
        private SubwindowRouter _sr;
        public PageRegister2(SubwindowRouter sr)
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
            var password = NewPassword.Password.ToCharArray();
            var passconfirm = NewConfirm.Password.ToCharArray();
            var username = Username.Text;
            var display = DisplayName.Text;
            var email = Email.Text;

            _sr.InteractiveTask(() =>
            {
                int ret;

                
                if (!password.SequenceEqual(passconfirm))
                {
                    Growl.Error("密码输入不一致");
                    return;
                }

                if (password.Length < 8)
                {
                    Growl.Error("请使用八位及以上长度的密码");
                    return;
                }

                if (!password.Intersect("0123456789".ToCharArray()).Any())
                {
                    Growl.Error("密码应包含数字");
                    return;
                }

                if (!password.Intersect("ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray()).Any())
                {
                    Growl.Error("密码应包含大写字母");
                    return;
                }

                if (!password.Intersect("abcdefghijklmnopqrstuvwxyz".ToCharArray()).Any())
                {
                    Growl.Error("密码应包含小写字母");
                    return;
                }

                _sr.Procedure = null;
                var local = new RegisterProcedure();

                try
                {
                    ret = local.Step1(_sr.Service, _sr.Character, username, display, email,
                        Encoding.UTF8.GetBytes(password), Helper.GetTrace());

                    ret = CheckVendor.NotValidResponseCode(ret) ? ret : local.Step2();

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

                if (CheckVendor.NotValidResponseCode(ret))
                {
                    Growl.Error(MessageVendor.FormatError(ret));
                }
                else
                {
                    Growl.Success("欢迎，" + (!string.IsNullOrWhiteSpace(_sr.Dispname) ? _sr.Dispname : _sr.Username));
                    SubwindowNavigator.Navigate(SubwindowPage.LoggedIn);
                }
            });
        }
    }
}
