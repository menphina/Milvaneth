using Milvaneth.Server.Models;
using Milvaneth.Server.Token;

namespace Milvaneth.Server.Service
{
    public interface IAuthentication
    {
        void EnsureAccount(AccountData account, PrivilegeConfig usage, int operation, int karma, string message,
            string ip);

        void EnsureToken(TokenPayload token, TokenPurpose usage, int operation, int karma, out AccountData account);

        void EnsureKey(KeyStore keyInfo, KeyUsage usage, int operation, int karma, string message, 
            string ip);
    }
}
