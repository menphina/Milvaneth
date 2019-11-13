using Milvaneth.Common;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkRetainerSumEnd : NetworkMessageProcessor, IResult
    {
        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkRetainerSumEnd);
        }

        public new static unsafe NetworkRetainerSumEnd Consume(byte[] data, int offset)
        {
            // we just need to know player is logging out
            return new NetworkRetainerSumEnd();
        }
    }
}