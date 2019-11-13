using HandyControl.Controls;
using System.Windows;
using System.Windows.Controls;

namespace Milvaneth.Subwindow
{
    /// <summary>
    /// Interaction logic for PageRegister1.xaml
    /// </summary>
    public partial class PageRegister1 : Page
    {
        private SubwindowRouter _sr;
        public PageRegister1(SubwindowRouter sr)
        {
            _sr = sr;
            InitializeComponent();

            _sr.InteractiveTask(() =>
            {
                _sr.Service = null;
                _sr.Character = null;

                if (!SubwindowDataCollector.Collect(5 * 60 * 1000, out _sr.Service, out _sr.Character))
                {
                    Growl.Error("等待超时，请返回上一页重试");

                    return;
                }

                if (_sr.Service != null && _sr.Character != null)
                {
                    SubwindowNavigator.Navigate(SubwindowPage.Register2);
                }
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.LoggedOut));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.Register2));
        }
    }
}
