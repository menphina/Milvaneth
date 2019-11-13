using Milvaneth.Common.Logging;
using Milvaneth.Communication.Vendor;
using Milvaneth.Interactive;
using Milvaneth.Subwindow;
using Milvaneth.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Application = System.Windows.Application;

namespace Milvaneth
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex _mutexwnd = new Mutex(true, "milmainwnd_mutex");

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var initWindow = new Initializing();
            initWindow.Show();

            Logger.Initialize(false, false);

            InlineLogic.InitializeLogic();

            var startMinimized = SupportStatic.RunMinimized(e.Args);

            var mainWindow = new MainWindow(_mutexwnd, out var hidden);

            initWindow.Close();

            if (!startMinimized && !hidden)
            {
                mainWindow.Show();
                mainWindow.Topmost = true;
                mainWindow.Topmost = false;
            }
            else
            {
                var am = new AccountManagement();
                am.Show();
                am.Topmost = true;
                am.Topmost = false;
                am.Closing += (o, args) =>
                {
                    if (!ApiVendor.HasToken())
                    {
                        InlineLogic.NotLoggedInLogic();
                        Environment.Exit(0);
                    }
                };

                Task.Run(() =>
                {
                    while (!ApiVendor.HasToken())
                    {
                        Thread.Sleep(1000);
                    }

                    if (Dispatcher?.CheckAccess() ?? true)
                    {
                        mainWindow.Show();
                        mainWindow.ShowOverlay();
                        mainWindow.Topmost = true;
                        mainWindow.Topmost = false;
                        return;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        mainWindow.Show();
                        mainWindow.ShowOverlay();
                        mainWindow.Topmost = true;
                        mainWindow.Topmost = false;
                    });
                });
            }
        }

        [STAThread]
        public static void Main()
        {
            if (_mutexwnd.WaitOne(TimeSpan.Zero, true))
            {
                App app = new App();
                app.InitializeComponent();
                app.Run();
            }
            else
            {
                NativeMethods.PostMessage(
                    (IntPtr)NativeMethods.HWND_BROADCAST,
                    NativeMethods.WM_SHOWME,
                    IntPtr.Zero,
                    IntPtr.Zero);
            }
        }
    }
}
