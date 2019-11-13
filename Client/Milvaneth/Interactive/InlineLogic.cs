using Milvaneth.Common;
using Milvaneth.Common.Communication.Version;
using Milvaneth.Communication.Procedure.Version;
using Milvaneth.Communication.Vendor;
using Milvaneth.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;

namespace Milvaneth.Interactive
{
    internal static class InlineLogic
    {
        public static void UpdateLogic(bool hideNoUpdateMessage = true, bool includeOptional = true)
        {
            var up = new UpdateProcedure();
            var needUpdate = false;
            VersionInfo info = null;
            int ret;

            try
            {
                ret = up.Step1(out needUpdate, out info);
            }
            catch (HttpRequestException e)
            {
                ret = 02_0000 + (int)e.Data["StatusCode"];
            }

            if (CheckVendor.NotValidResponseCode(ret))
            {
                MessageBox.Show($"客户端检查更新失败：{MessageVendor.FormatError(ret)}", "Milvaneth 客户端更新");
                return;
            }

            if (!needUpdate)
            {
                if (!hideNoUpdateMessage)
                {
                    MessageBox.Show($"已经是最新版本", "Milvaneth 客户端更新");
                }

                return;
            }

            if (!info.ForceUpdate)
            {
                if (!includeOptional)
                    return;

                var result =
                    MessageBox.Show(
                        $"有可用新版本 {info.EligibleMilvanethVersion} (D-{info.EligibleDataVersion} / G-{info.EligibleGameVersion})\n" +
                        $"版本说明：{info.DisplayMessage}\n" + "是否现在更新？", "Milvaneth 客户端更新", MessageBoxButton.YesNo);

                if (result == MessageBoxResult.No)
                    return;
            }
            else
            {
                Task.Run(() => MessageBox.Show(
                    $"有可用新版本 {info.EligibleMilvanethVersion} (D-{info.EligibleDataVersion} / G-{info.EligibleGameVersion})\n" +
                    $"版本说明：{info.DisplayMessage}\n" + "本版本为必要更新，将自动下载并安装更新\n请不要关闭或退出程序，程序将在更新完成后自动重启", "Milvaneth 客户端更新", MessageBoxButton.OK));
            }

            VersionDownload download = null;

            try
            {
                ret = up.Step2(out download);
            }
            catch (HttpRequestException e)
            {
                ret = 02_0000 + (int)e.Data["StatusCode"];
            }

            if (CheckVendor.NotValidResponseCode(ret))
            {
                MessageBox.Show($"客户端下载更新失败：{MessageVendor.FormatError(ret)}", "Milvaneth 客户端更新");
                return;
            }

            var patches = UpdateVendor.DownloadPatches(download, null);
            UpdateVendor.WaitAllFinish(patches);

            if (!info.ForceUpdate)
            {
                MessageBox.Show($"更新即将开始，请不要关闭或退出程序\n程序将在更新完成后自动重启", "Milvaneth 客户端更新");
            }

            SupportStatic.ExitAll();
            UpdateVendor.InvokeUpdateAndExitProgram(download, true);
        }

        public static void InitializeLogic()
        {
            try
            {
                if (!SupportStatic.FirstRunPrepare())
                {
                    MessageBox.Show(
                        "无法找到初始数据文件\n\n请确保文件名近似 milinit_20190101.pack 的数据文件\n已经被下载并被放置于本软件的根目录下\n\n您通常可以在本软件的下载页面找到数据文件的下载链接",
                        "初始化失败");
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("软件初始化出现错误：\n" + ex, "初始化失败");
                Environment.Exit(0);
            }

            try
            {
                if (!SupportStatic.CheckFileIntegrity())
                {
                    MessageBox.Show("文件完整性校验异常，请重新下载本程序或手动覆盖受损文件\n您可能需要手动删除数据文件夹", "初始化失败");
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("软件缺少运行必要的文件，请重新下载安装本程序" + ex, "初始化失败");
                Environment.Exit(0);
            }

            try
            {
                var exeName = "Milvaneth.Cmd.exe";
                var proc = Process.GetCurrentProcess();
                var path = proc.GetMainModuleFileName() ??
                           throw new InvalidOperationException("Current process has no main module");

                var _exePath = Path.Combine(Path.GetDirectoryName(path), exeName);

                if (!(FirewallRegister.RegisterToFirewall(_exePath, "Milvaneth Core Component") &&
                      FirewallRegister.RegisterToFirewall(proc.GetMainModuleFileName(), "Milvaneth Client")))
                {
                    Task.Run(FirewallLogic);
                }
            }
            catch
            {
                Task.Run(FirewallLogic);
            }
        }

        public static void GlobalMessageLogic()
        {
            if (!string.IsNullOrWhiteSpace(MilvanethConfig.Store.Global.CustomMessage))
            {
                MessageBox.Show(MilvanethConfig.Store.Global.CustomMessage, "Milvaneth 客户端通知");
            }
        }

        public static void FirewallLogic()
        {
            var result = MessageBox.Show("无法自动注册防火墙规则，是否手工注册？", "提示", MessageBoxButton.YesNo);

            if (result != MessageBoxResult.Yes) return;

            Process.Start("control.exe", "/name Microsoft.WindowsFirewall /page pageConfigureApps");
            MessageBox.Show("请在稍后打开的页面中添加并允许 Milvaneth.exe 和 Milvaneth.Cmd.exe\n使用私人和公用网络\n\n并请在完成上述操作后重新启动本程序", "提示", MessageBoxButton.OK);
        }

        public static void PostSignalNotifyLogic(Signal sig)
        {
            switch (sig)
            {
                case Signal.ClientInsuffcientPrivilege:
                    MessageBox.Show($"无法检测游戏进程，权限不足\n请尝试以管理员权限重新启动程序", "权限不足");
                    break;

                case Signal.ClientNetworkFail:
                    MessageBox.Show($"无法检测网络数据\n您可能需要检视网络设置", "无法访问网络");
                    break;

                case Signal.MilvanethNeedUpdate:
                    MessageBox.Show($"无法正确检测游戏配置\n您可能需要检查更新", "版本错误");
                    break;

                default:
                    break;
            }
        }

        public static void EndpointSettingLogic(bool succ)
        {
            if (!succ)
            {
                MessageBox.Show("剪贴板中没有 API 访问点信息", "API 访问点设置失败");
            }
            else
            {
                MessageBox.Show("成功设置 API 访问点信息\n您需要重启程序以使该设置生效", "API 访问点设置成功");
            }
        }

        public static void NotLoggedInLogic()
        {
            MessageBox.Show("请登录账户以使用本程序");
        }

        public static void ServerUnreachableLogic()
        {
            MessageBox.Show("无法访问 Milvaneth 数据平台，请检查您的网络连接后重试。\n\n如果该问题持续存在，您可以尝试下载新版或联系作者。", "数据服务错误");
            Environment.Exit(0);
        }
    }
}
