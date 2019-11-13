using MessagePack;
using System;
using System.Collections.Generic;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class ChatlogResult : IResult
    {
        public ChatlogResult(List<ChatlogLine> lines, int rem)
        {
            LogLines = lines ?? new List<ChatlogLine>();
            Remaining = rem > -1 ? rem : -1;

            if (LogLines.Count > 0)
            {
                FromLogTime = Helper.UnixTimeStampToDateTime(LogLines[0].Timestamp, false);
                ToLogTime = Helper.UnixTimeStampToDateTime(LogLines[LogLines.Count - 1].Timestamp, false);
            }
            else
            {
                FromLogTime = Helper.UnixTimeStampToDateTime(0, false);
                ToLogTime = Helper.UnixTimeStampToDateTime(0, false);
            }
        }

        [Key(0)]
        public List<ChatlogLine> LogLines { get; }
        [Key(1)]
        public int Remaining { get; }
        [IgnoreMember]
        public SafeDateTime FromLogTime { get; }
        [IgnoreMember]
        public SafeDateTime ToLogTime { get; }
    }
}
