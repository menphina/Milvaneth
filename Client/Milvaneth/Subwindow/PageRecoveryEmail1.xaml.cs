using HandyControl.Controls;
using Milvaneth.Communication.Vendor;
using System.Windows;
using System.Windows.Controls;

namespace Milvaneth.Subwindow
{
    /// <summary>
    /// Interaction logic for PageRecoveryEmail1.xaml
    /// </summary>
    public partial class PageRecoveryEmail1 : Page
    {
        private SubwindowRouter _sr;
        public PageRecoveryEmail1(SubwindowRouter sr)
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
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.RecoveryEntry));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var email = Email.Text;

            _sr.InteractiveTask(() =>
            {
                if (CheckVendor.NotValidEmail(email))
                {
                    Growl.Error("无效邮件地址");
                    return;
                }

                _sr.Email = email;

                SubwindowNavigator.Navigate(SubwindowPage.RecoveryEmail2);
            });
        }
    }
}
