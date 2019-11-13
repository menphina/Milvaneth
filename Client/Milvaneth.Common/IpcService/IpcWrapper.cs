using SharedMemory;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Milvaneth.Common
{
    public class IpcWrapper : IDisposable
    {
        public long SendSuccess => _sendSuccess;
        public long RecvSuccess => _recvSuccess;
        public long SendSkipped => _sendSkipped;
        public long RecvSkipped => _recvSkipped;

        public OnDataReceivedDelegate OnReceived;
        public readonly WorkingMode Mode;

        private int _timeout;
        private CircularBuffer _sender;
        private CircularBuffer _reciever;
        private BlockingCollection<byte[]> _sendQueue;
        private long _sendSuccess;
        private long _recvSuccess;
        private long _sendSkipped;
        private long _recvSkipped;
        private bool _stopping;

        public IpcWrapper(WorkingMode mode, int busId, int timeout = 1000)
        {
            ResetCounters();
            _timeout = timeout;
            Mode = mode;

            switch (mode)
            {
                case WorkingMode.SignalServer:
                    _sender = new CircularBuffer($@"milsigs2c_{busId}", 8, 65536 * sizeof(byte));
                    _reciever = new CircularBuffer($@"milsigc2s_{busId}", 8, 65536 * sizeof(byte));
                    break;
                case WorkingMode.SignalClient:
                    _sender = new CircularBuffer($@"milsigc2s_{busId}");
                    _reciever = new CircularBuffer($@"milsigs2c_{busId}");
                    break;
                case WorkingMode.DataServer:
                    _sender = null;
                    _reciever = new CircularBuffer($@"mildatc2s_{busId}", 64, 65536 * sizeof(byte)); ;
                    break;
                case WorkingMode.DataClient:
                    _sender = new CircularBuffer($@"mildatc2s_{busId}");
                    _reciever = null;
                    break;
            }

            if(_sender != null)
                Task.Run(StartSending);

            if(_reciever != null)
                Task.Run(StartReceiving);
        }

        public void Dispose()
        {
            this._sendQueue?.CompleteAdding();
            _stopping = true;

            try { _sender?.Dispose(); } catch { /* ignored*/ }
            try { _reciever?.Dispose(); } catch { /* ignored*/ }
            _sender = null;
            _reciever = null;
        }

        public void ResetCounters()
        {
            Interlocked.Exchange(ref _sendSuccess, 0L);
            Interlocked.Exchange(ref _recvSuccess, 0L);
            Interlocked.Exchange(ref _sendSkipped, 0L);
            Interlocked.Exchange(ref _recvSkipped, 0L);
        }

        public void Enqueue(byte[] data)
        {
            if(_sender == null)
                throw new InvalidOperationException("No send allowed");

            this._sendQueue.Add(data);
        }

        private void StartSending()
        {
            _sendQueue = new BlockingCollection<byte[]>();

            foreach (var data in this._sendQueue.GetConsumingEnumerable())
            {
                if ((_sender?.Write(data, 0, _timeout) ?? 0) > 0)
                {
                    Interlocked.Increment(ref _sendSuccess);
                }
                else
                {
                    Interlocked.Increment(ref _sendSkipped);
                }
            }
        }

        private void StartReceiving()
        {
            while(!_stopping)
            {
                var buffer = new byte[65536];
                var amount = _reciever?.Read(buffer, 0, _timeout) ?? 0;
                if (amount > 0)
                {
                    var output = new byte[amount];
                    Buffer.BlockCopy(buffer, 0, output, 0, amount);
                    OnReceived?.Invoke(output);
                    Interlocked.Increment(ref _recvSuccess);
                }
                else
                {
                    Interlocked.Increment(ref _recvSkipped);
                }
            }
        }
    }

    public enum WorkingMode
    {
        SignalServer,
        SignalClient,
        DataServer,
        DataClient,
    }
}