using Milvaneth.Service;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Milvaneth
{
    /// <summary>
    /// Interaction logic for LogPage.xaml
    /// </summary>
    public partial class LogPage : Page
    {
        private readonly BindingRouter br;
        public LogPage(BindingRouter dataContext)
        {
            InitializeComponent();
            CommandBindings.Add(new CommandBinding(BindingRouter.LogCommandLineEnterCommand, OnCommandLineEnter));
            DataContext = dataContext;
            br = dataContext;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LogText.ScrollToEnd();
            br.LogRealtimeRefresh = true;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            br.LogRealtimeRefresh = false;
        }

        public void OnCommandLineEnter(object sender, ExecutedRoutedEventArgs e)
        {
            // not implemented
        }

        private void LogText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            LogText.ScrollToEnd();
        }
    }
}
