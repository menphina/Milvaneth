using HandyControl.Controls;
using Milvaneth.Common;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Milvaneth.Subwindow
{
    /// <summary>
    /// Interaction logic for PageConsent.xaml
    /// </summary>
    public partial class PageConsent : Page
    {
        private SubwindowRouter _sr;
        public PageConsent(SubwindowRouter sr)
        {
            _sr = sr;
            InitializeComponent();
            var aboutStream = Application
                .GetResourceStream(new Uri("pack://application:,,,/Milvaneth;component/about.txt")).Stream;
            var stream = new StreamReader(aboutStream);
            Note.Text = stream.ReadToEnd();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _sr.InteractiveTask(() => SubwindowNavigator.Navigate(SubwindowPage.Welcome));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            _sr.InteractiveTask(() =>
            {
                try
                {
                    var path = Helper.GetMilFilePathRaw(".consent");

                    using (new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);
                    }
                }
                catch
                {
                    Growl.Warning("无法创建标记文件，您未来可能需要再次阅读此内容");
                }


                SubwindowNavigator.Navigate(SubwindowPage.LoggedOut);

            });
        }
    }
}
