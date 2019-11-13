using Milvaneth.Service;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Milvaneth.Common;

namespace Milvaneth.Overlay
{
    public static class OverlayLoader
    {
        public static bool TryLoadOverlay(string assemblyPath, BindingRouter br, out OverlayBase ob)
        {
            if (assemblyPath == null)
            {
                ob = LoadOverlay(
                    Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().GetMainModuleFileName()),
                        "Milvaneth.Overlay.dll"), br);
                LoggingManagementService.WriteLine($"Loaded default overlay", "OvlMgmt");
                return true;
            }

            try
            {
                ob = LoadOverlay(assemblyPath, br);
                LoggingManagementService.WriteLine($@"Loaded custom overlay at ""{assemblyPath}""", "OvlMgmt");
                return true;
            }
            catch
            {
               ob = LoadOverlay(
                    Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().GetMainModuleFileName()),
                        "Milvaneth.Overlay.dll"), br);
            }

            LoggingManagementService.WriteLine($@"Loaded custom overlay failed. Using default overlay", "OvlMgmt");
            return false;
        }

        private static OverlayBase LoadOverlay(string assemblyPath, BindingRouter br)
        {
            var assembly = Assembly.LoadFile(assemblyPath);
            var types = GetTypeByName(assembly);
            if (types.Length != 1)
                throw new InvalidOperationException("Unable to determine overlay class based on OverlayBase");
            var type = types[0];
            return (OverlayBase)Activator.CreateInstance(type, br);
        }

        private static Type[] GetTypeByName(Assembly assembly)
        {
            var returnVal = new List<Type>();

            var assemblyTypes = assembly.GetTypes();
            foreach (var t in assemblyTypes)
            {
                if (t.BaseType != null && t.BaseType == typeof(OverlayBase))
                {
                    returnVal.Add(t);
                }
            }

            return returnVal.ToArray();
        }
    }
}
