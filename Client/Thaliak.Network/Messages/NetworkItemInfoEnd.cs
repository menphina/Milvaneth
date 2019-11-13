using Milvaneth.Common;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkItemInfoEnd : NetworkMessageProcessor, IResult
    {
        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkItemInfoEnd);
        }

        public new static unsafe NetworkItemInfoEnd Consume(byte[] data, int offset)
        {
            // we just need to know player is logging out
            return new NetworkItemInfoEnd();
        }
    }
}