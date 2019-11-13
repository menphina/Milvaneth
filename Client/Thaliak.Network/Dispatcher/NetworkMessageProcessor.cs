namespace Thaliak.Network
{
    public abstract class NetworkMessageProcessor
    {
        public static int GetMessageId()
        {
            return -1;
        }

        public static unsafe NetworkMessageProcessor Consume(byte[] data, int offset)
        {
            return default;
        }
    }
}
