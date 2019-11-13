using Window = System.Windows.Window;

namespace Milvaneth.Subwindow
{
    /// <summary>
    /// Interaction logic for AccountManagement.xaml
    /// </summary>
    public partial class AccountManagement : Window
    {
        private SubwindowRouter sr;

        public AccountManagement()
        {
            sr = new SubwindowRouter();

            InitializeComponent();
            this.DataContext = sr;

            SubwindowNavigator.PrepareDefaultPage(sr, AccountFrame, Dispatcher);
        }
    }
}
