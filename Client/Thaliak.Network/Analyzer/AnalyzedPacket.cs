using Thaliak.Network.Filter;

namespace Thaliak.Network.Analyzer
{
    public class AnalyzedPacket
    {
        public int Length { get; }
        public long Timestamp { get; }
        public MessageAttribute RouteMark { get; }
        public byte[] Message { get; }

        public AnalyzedPacket(int len, byte[] msg, long time, MessageAttribute mark)
        {
            Length = len;
            Message = msg;
            Timestamp = time;
            RouteMark = mark;
        }
    }
}
