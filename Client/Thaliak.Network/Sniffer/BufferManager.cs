// Modifications copyright (C) 2019 Menphina

using Milvaneth.Common;
using System;
using System.Net.Sockets;

namespace Thaliak.Network.Sniffer
{
    /// <summary>
    /// Fixed pool of buffers for use by async socket connections, to avoid connections creating their
    /// own buffers (which leads to a lot of pinned buffers, causing heap fragmentation)
    /// </summary>
    public class BufferManager
    {
        private readonly byte[] buffer;
        private readonly int segmentSize;
        private int nextOffset;

        public BufferManager(int segmentSize, int numSegments)
        {
            this.segmentSize = segmentSize;
            this.buffer = new byte[segmentSize * numSegments];
        }

        public void AssignSegment(SocketAsyncEventArgs e)
        {
            if (this.nextOffset + this.segmentSize > this.buffer.Length)
            {
                IpcClient.SendSignal(Signal.InternalException, new[] {"Buffer exhausted"});
                IpcClient.SendSignal(Signal.MilvanethComponentExit, new[] {"Network", "Sniffer"});
                throw new InsufficientMemoryException("Buffer exhausted");
            }

            e.SetBuffer(this.buffer, this.nextOffset, this.segmentSize);
            this.nextOffset += this.segmentSize;
        }
    }
}