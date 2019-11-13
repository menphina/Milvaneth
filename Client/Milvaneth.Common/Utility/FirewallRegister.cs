using NetFwTypeLib;
using System;
using System.Diagnostics;

namespace Milvaneth.Common
{
    public class FirewallRegister
    {
        //control.exe /name Microsoft.WindowsFirewall /page pageConfigureApps
        public static bool RegisterToFirewall(string exePath, string ruleName)
        {
            try
            {
                var netFwMgr = GetInstance<INetFwMgr>("HNetCfg.FwMgr");

                if (!netFwMgr.LocalPolicy.CurrentProfile.FirewallEnabled) return true;

                var netAuthApps = netFwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications;
                var isExists = false;

                foreach (var netAuthAppObject in netAuthApps)
                {
                    if (!(netAuthAppObject is INetFwAuthorizedApplication naa) 
                        || naa.ProcessImageFileName != exePath 
                        || !naa.Enabled) continue;

                    isExists = true;
                    break;
                }

                if (isExists) return true;
                
                var netAuthApp = GetInstance<INetFwAuthorizedApplication>("HNetCfg.FwAuthorizedApplication");

                netAuthApp.Enabled = true;
                netAuthApp.Name = ruleName;
                netAuthApp.ProcessImageFileName = exePath;
                netAuthApp.Scope = NET_FW_SCOPE_.NET_FW_SCOPE_ALL;

                netAuthApps.Add(netAuthApp);
                
                return true;
            }
            catch
            {
                return false;
            }

            T GetInstance<T>(string typeName)
            {
                return (T)Activator.CreateInstance(Type.GetTypeFromProgID(typeName));
            }
        }
    }
}
