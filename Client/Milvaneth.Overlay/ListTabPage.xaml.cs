using System.Windows.Controls;

namespace Milvaneth.Overlay
{
    /// <summary>
    /// Interaction logic for ListTabPage.xaml
    /// </summary>
    public partial class ListTabPage : Page
    {
        private ListingPresenterViewModel lpvm;
        public ListTabPage(ListingPresenterViewModel dataContext)
        {
            InitializeComponent();
            this.ElementView.DataContext = dataContext;
            lpvm = dataContext;
        }
    }
}
