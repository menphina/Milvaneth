using System;
using System.ComponentModel;
using System.IO;
using Milvaneth.Service;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Milvaneth
{
    /// <summary>
    /// Interaction logic for about.xaml
    /// </summary>
    public partial class AboutPage : Page
    {
        private readonly BindingRouter br;
        private AboutViewModel avm;
        public AboutPage(BindingRouter dataContext)
        {
            avm = new AboutViewModel();

            InitializeComponent();

            var aboutStream = Application
                .GetResourceStream(new Uri("pack://application:,,,/Milvaneth;component/about.txt")).Stream;
            var sr = new StreamReader(aboutStream);
            AboutBox.Text = sr.ReadToEnd();

#if DEBUG
            AboutName.Text = "Milvaneth 客户端 [测试版]";
#endif

            DataContext = avm;
            br = dataContext;

            avm.FormattedVersionString =
                $"版本 {br.AboutMilvanethVersion} (D-{br.AboutDataVersion} / G-{br.AboutGameVersion})";
            dataContext.PropertyChanged += OnPropertyChanged;
        }

        protected void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e == null) return;

            switch (e.PropertyName)
            {
                case nameof(br.AboutDataVersion):
                case nameof(br.AboutGameVersion):
                case nameof(br.AboutMilvanethVersion):
                    avm.FormattedVersionString =
                        $"版本 {br.AboutMilvanethVersion} (D-{br.AboutDataVersion} / G-{br.AboutGameVersion})";
                    break;

                default:
                    break;
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AboutTag.Visibility = Visibility.Hidden;
            AboutBox.Visibility = Visibility.Visible;
        }
    }

    public class AboutViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _formattedVersionString;
        public string FormattedVersionString
        {
            get => _formattedVersionString;
            set
            {
                _formattedVersionString = value;
                OnPropertyChanged(nameof(FormattedVersionString));
            }
        }

        internal void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
