using Milvaneth.Common;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkLogoutCancel : NetworkMessageProcessor, IResult
    {
        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkLogoutCancel);
        }

        public new static unsafe NetworkLogoutCancel Consume(byte[] data, int offset)
        {
            // we just need to know player is logging back
            return new NetworkLogoutCancel();
        }
    }
}
