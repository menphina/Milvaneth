using System.Windows.Controls;

namespace Milvaneth.Overlay
{
    /// <summary>
    /// Interaction logic for HistTabPage.xaml
    /// </summary>
    public partial class HistTabPage : Page
    {
        private HistoryPresenterViewModel hpvm;
        public HistTabPage(HistoryPresenterViewModel dataContext)
        {
            InitializeComponent();
            this.ElementView.DataContext = dataContext;
            hpvm = dataContext;
        }
    }
}
