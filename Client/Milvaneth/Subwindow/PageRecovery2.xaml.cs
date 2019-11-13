using HandyControl.Controls;
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
    /// Interaction logic for PageRecovery2.xaml
    /// </summary>
    public partial class PageRecovery2 : Page
    {
        private SubwindowRouter _sr;
        public PageRecovery2(SubwindowRouter sr)
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

                if (string.IsNullOrWhiteSpace(email))
                {
                    email = null;
                }

                try
                {
                    if (!(_sr.Procedure is RecoveryProcedure local))
                    {
                        ret = 02_0009;
                        goto FAIL;
                    }

                    ret = local.Step3(Encoding.UTF8.GetBytes(password), email);

                    ret = CheckVendor.NotValidResponseCode(ret) ? ret : local.Step4();

                    ret = CheckVendor.NotValidResponseCode(ret) ? ret : local.Step5();

                    byte[] token = null;

                    ret = CheckVendor.NotValidResponseCode(ret) ? ret : local.Step6(out token);

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
                    Growl.Success("已修改密码并登录");
                    _sr.Procedure = null;
                    SubwindowNavigator.Navigate(SubwindowPage.LoggedIn);
                }
            });
        }
    }
}
