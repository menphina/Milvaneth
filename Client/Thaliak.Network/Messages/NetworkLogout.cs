using Milvaneth.Common;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkLogout : NetworkMessageProcessor, IResult
    {
        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkLogout);
        }

        public new static unsafe NetworkLogout Consume(byte[] data, int offset)
        {
            // we just need to know player is logging out
            return new NetworkLogout();
        }
    }
}
