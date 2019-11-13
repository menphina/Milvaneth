using Milvaneth.Common;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkUpdateHpMpTp : NetworkMessageProcessor, IResult
    {
        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkUpdateHpMpTp);
        }

        public new static unsafe NetworkUpdateHpMpTp Consume(byte[] data, int offset)
        {
            // we just need to know player is logging out
            return new NetworkUpdateHpMpTp();
        }
    }
}