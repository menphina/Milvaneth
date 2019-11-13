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
    /// Interaction logic for PageChange2.xaml
    /// </summary>
    public partial class PageChange2 : Page
    {
        private SubwindowRouter _sr;
        public PageChange2(SubwindowRouter sr)
        {
            _sr = sr;
            InitializeComponent();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var password = NewPassword.Password.ToCharArray();
            var passconfirm = NewConfirm.Password.ToCharArray();

            _sr.InteractiveTask(() =>
            {
                int ret = 00_0000;
                
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

                try
                {
                    if (!(_sr.Procedure is ChangeProcedure local))
                    {
                        ret = 02_0009;
                        goto FAIL;
                    }

                    ret = CheckVendor.NotValidResponseCode(ret) ? ret : local.Step4(Encoding.UTF8.GetBytes(password));

                    ret = CheckVendor.NotValidResponseCode(ret) ? ret : local.Step5();

                    ret = CheckVendor.NotValidResponseCode(ret) ? ret : local.Step6();

                    byte[] token = null;

                    ret = CheckVendor.NotValidResponseCode(ret) ? ret : local.Step7(out token);

                    _sr.RenewToken = token;
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
                    Growl.Success("成功修改密码");
                    SubwindowNavigator.Navigate(SubwindowPage.LoggedIn);
                }
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.LoggedIn));
        }
    }
}
