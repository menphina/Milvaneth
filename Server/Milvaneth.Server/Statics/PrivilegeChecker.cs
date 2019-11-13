using Milvaneth.Server.Models;
using Milvaneth.Server.Token;

namespace Milvaneth.Server.Statics
{
    public static class PrivilegeChecker
    {
        public static bool IsSatisfied(this KeyUsage requirement, KeyUsage enforcement)
        {
            return ((requirement.AccessData & enforcement.AccessData) == requirement.AccessData) &&
                   ((requirement.BatchRead & enforcement.BatchRead) == requirement.BatchRead) &&
                   ((requirement.BatchWrite & enforcement.BatchWrite) == requirement.BatchWrite) &&
                   ((requirement.ChangePassword & enforcement.ChangePassword) == requirement.ChangePassword) &&
                   ((requirement.CreateSession & enforcement.CreateSession) == requirement.CreateSession) &&
                   ((requirement.GetChangeToken & enforcement.GetChangeToken) == requirement.GetChangeToken) &&
                   ((requirement.ProveIdentity & enforcement.ProveIdentity) == requirement.ProveIdentity) &&
                   ((requirement.RenewSession & enforcement.RenewSession) == requirement.RenewSession);
        }

        public static bool IsSatisfied(this PrivilegeConfig requirement, PrivilegeConfig enforcement)
        {
            return ((requirement.AccessData & enforcement.AccessData) == requirement.AccessData) &&
                   ((requirement.Login & enforcement.Login) == requirement.Login) &&
                   ((requirement.IgnoreKarma & enforcement.IgnoreKarma) == requirement.IgnoreKarma) &&
                   ((requirement.AccessStatics & enforcement.AccessStatics) == requirement.AccessStatics) &&
                   ((requirement.Debug & enforcement.Debug) == requirement.Debug) &&
                   ((requirement.BatchRead & enforcement.BatchRead) == requirement.BatchRead) &&
                   ((requirement.BatchWrite & enforcement.BatchWrite) == requirement.BatchWrite) &&
                   ((requirement.AccountOperation & enforcement.AccountOperation) == requirement.AccountOperation) &&
                   ((requirement.ReleaseUpdate & enforcement.ReleaseUpdate) == requirement.ReleaseUpdate) &&
                   ((requirement.DeleteRecord & enforcement.DeleteRecord) == requirement.DeleteRecord) &&
                   ((requirement.AccountManagement & enforcement.AccountManagement) == requirement.AccountManagement);
        }

        public static bool IsSatisfied(this TokenPurpose requirement, TokenPurpose enforcement)
        {
            return (requirement & enforcement) == requirement;
        }

        public static bool HasSuspended(this AccountData account)
        {
            return account.SuspendUntil != null;
        }
    }
}
