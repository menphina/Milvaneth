using System.Windows;
using System.Windows.Controls;

namespace Milvaneth.Subwindow
{
    /// <summary>
    /// Interaction logic for PageRecovery1.xaml
    /// </summary>
    public partial class PageRecovery1 : Page
    {
        private SubwindowRouter _sr;
        public PageRecovery1(SubwindowRouter sr)
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
            _sr.Username = Username.Text;

            _sr.InteractiveTask(() =>
            {
                SubwindowNavigator.Navigate(SubwindowPage.RecoveryEntry);
            });
        }
    }
}
