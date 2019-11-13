// Modifications copyright (C) 2019 Menphina

using System;
using Thaliak.Network.Filter;

namespace Thaliak.Network.Sniffer
{
    public class TimestampedData
    {
        public DateTime Timestamp { get; }
        public byte[] Data { get; }
        public int Protocol { get; set; }
        public int HeaderLength { get; set; }
        public MessageAttribute RouteMark { get; set; }

        public TimestampedData(DateTime timestamp, byte[] data)
        {
            this.Timestamp = timestamp;
            this.Data = data;
        }
    }
}