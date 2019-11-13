using System.Windows;
using System.Windows.Controls;

namespace Milvaneth.Subwindow
{
    /// <summary>
    /// Interaction logic for PageWelcome.xaml
    /// </summary>
    public partial class PageWelcome : Page
    {
        private SubwindowRouter _sr;
        public PageWelcome(SubwindowRouter sr)
        {
            _sr = sr;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.Consent));
        }
    }
}
