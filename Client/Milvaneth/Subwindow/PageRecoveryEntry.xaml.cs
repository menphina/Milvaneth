using System.Windows;
using System.Windows.Controls;

namespace Milvaneth.Subwindow
{
    /// <summary>
    /// Interaction logic for PageRecoveryEntry.xaml
    /// </summary>
    public partial class PageRecoveryEntry : Page
    {
        private SubwindowRouter _sr;
        public PageRecoveryEntry(SubwindowRouter sr)
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
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.RecoveryAccount1));
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.RecoveryEmail1));
        }
    }
}
