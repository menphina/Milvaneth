// Modifications copyright (C) 2019 Menphina

using System.Security.Principal;

namespace Thaliak.Network.Utilities
{
    public static class SystemInformation
    {
        public static bool IsAdmin()
        {
            return new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
}