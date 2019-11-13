using Milvaneth.Common;

namespace Thaliak.Network.Dispatcher
{
    public delegate IResult MessageConsumerDelegate(byte[] message, int offset);
    class RoutedMessage
    {
        public MessageConsumerDelegate Consumer { get; }
        public MessageDecoded Listener { get; }
        public int HeaderLength { get; }
        public byte[] Message { get; }
        public NetworkMessageHeader Header { get; }

        public RoutedMessage(NetworkMessageHeader header, int len, byte[] msg, MessageConsumerDelegate consumer, MessageDecoded listener)
        {
            Header = header;
            HeaderLength = len;
            Message = msg;
            Consumer = consumer;
            Listener = listener;
        }
    }
}
