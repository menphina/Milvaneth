using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class ChatlogLine
    {
        public ChatlogLine(long ts, int opcode, string sender, string msg)
        {
            Timestamp = ts;
            MessageType = opcode;
            Sender = sender;
            Message = msg;
        }
        [Key(0)]
        public long Timestamp { get; }
        [Key(1)]
        public int MessageType { get; }
        [Key(2)]
        public string Sender { get; }
        [Key(3)]
        public string Message { get; }

        public override string ToString()
        {
            // ACT FFXIV Log style
            return
                $"{Helper.UnixTimeStampToDateTime(Timestamp):s}.0000000+08:00|{MessageType:x4}|{Sender}|{Message.Replace('|', ':')}";
        }
    }
}
