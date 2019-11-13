using Milvaneth.Server.Models;
using System;

namespace Milvaneth.Server.Service
{
    public interface IApiKeySignService
    {
        KeyStore Sign(KeyUsage usage, int quota, AccountData account, DateTime from, DateTime to);
    }
}
