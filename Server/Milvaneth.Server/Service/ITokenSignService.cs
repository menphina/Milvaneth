using Milvaneth.Server.Token;

namespace Milvaneth.Server.Service
{
    public interface ITokenSignService
    {
        string Sign(TokenPayload payload);
        TokenPayload Decode(string token);
        bool TryDecode(string token, out TokenPayload payload);
    }
}
