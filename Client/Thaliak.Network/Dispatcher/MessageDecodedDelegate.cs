using Milvaneth.Common;

namespace Thaliak.Network.Dispatcher
{
    public delegate void MessageDecoded(NetworkMessageHeader header, IResult message);
}
