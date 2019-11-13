using System;

namespace Milvaneth.Server.Token
{
    [Flags]
    public enum TokenPurpose : byte
    {
        AccessToken = 1 << 0,
        RenewToken = 1 << 1,
        ChangeToken = 1 << 2,
        AuthToken = 1 << 3,
        RecoveryToken = 1 << 4,
    }
}
