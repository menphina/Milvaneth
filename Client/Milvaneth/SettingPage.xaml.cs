using Milvaneth.Common;
using Milvaneth.Interactive;
using Milvaneth.Service;
using Milvaneth.Subwindow;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Clipboard = System.Windows.Clipboard;

namespace Milvaneth
{
    /// <summary>
    /// Interaction logic for SettingPage.xaml
    /// </summary>
    public partial class SettingPage : Page
    {
        private readonly BindingRouter br;
        private delegate void UpdateTaskCallback(IEnumerable<string> list);

        public SettingPage(BindingRouter dataContext)
        {
            InitializeComponent();
            DataContext = dataContext;
            br = dataContext;

            Task.Run(() =>
            {
                for (;;)
                {
                    // use an independent source to maintain process list: less filter, more stable
                    UpdateTask();
                    Thread.Sleep(15000);
                }
            });
        }

        private void Button_Open_Folder(object sender, RoutedEventArgs e)
        {
            Process.Start(Helper.GetMilFilePathRaw(""));
        }

        private void Button_Select_Overlay(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.CheckFileExists = true;
            openFileDialog.Multiselect = false;
            openFileDialog.AddExtension = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.AutoUpgradeEnabled = true;
            openFileDialog.DefaultExt = ".dll";
            openFileDialog.Filter = @"Overlay 插件文件|*.dll;*.exe|所有文件|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                br.InternalOverlayAssemblyPath = openFileDialog.FileName;
            }
        }

        private void Button_Set_Endpoint(object sender, RoutedEventArgs e)
        {
            var str = Clipboard.GetText();

            // milvaneth://api.menphina.com:2020/milvaneth/
            if (string.IsNullOrWhiteSpace(str) ||
                !Uri.TryCreate(str, UriKind.Absolute, out var uri) ||
                !uri.Scheme.Equals("milvaneth", StringComparison.InvariantCultureIgnoreCase))
            {
                Task.Run(() => InlineLogic.EndpointSettingLogic(false));
                return;
            }

            var uriWithoutScheme = Uri.SchemeDelimiter + uri.Authority + uri.PathAndQuery + uri.Fragment;
            var ub = new UriBuilder(Uri.UriSchemeHttps + uriWithoutScheme);
            br.InternalPerferredApiEntry = ub.Uri.AbsoluteUri;
            Task.Run(() => InlineLogic.EndpointSettingLogic(true));
        }

        private void Button_View_Website(object sender, RoutedEventArgs e)
        {
            Process.Start(MilvanethConfig.Store.Global.ProjectUrl);
        }

        private void Button_Account_Login(object sender, RoutedEventArgs e)
        {
            var am = new AccountManagement();
            am.Show();
            am.Topmost = true;
            am.Topmost = false;
        }

        private void Button_Check_Update(object sender, RoutedEventArgs e)
        {
            Task.Run(() => InlineLogic.UpdateLogic(hideNoUpdateMessage:false));
        }

        private void Button_Attach_Game(object sender, RoutedEventArgs e)
        {
            var val = (string)GamePidList.SelectedValue;
            if (int.TryParse(val, out var pid))
            {
                SubprocessManagementService.SpawnSpecific(pid);
            }
        }

        private void UpdateCombo(IEnumerable<string> list)
        {
            GamePidList.Items.Clear();
            foreach (var item in list)
            {
                GamePidList.Items.Add(item);
            }
        }

        private void UpdateTask()
        {
            GamePidList.Dispatcher.Invoke(new UpdateTaskCallback(UpdateCombo),
                Helper.GetProcessList().Select(x => x.Id.ToString()));
        }
    }
}
