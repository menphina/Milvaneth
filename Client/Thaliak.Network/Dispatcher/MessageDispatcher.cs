using Milvaneth.Common;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Thaliak.Network.Analyzer;
using Thaliak.Network.Filter;

namespace Thaliak.Network.Dispatcher
{
    public class MessageDispatcher : IAnalyzerOutput
    {
        public const int OPMASK_LOBBY = 0x000A_0000;

        private const int HeaderLength = 32;

        private bool _isStopping;
        private long _messagesDispatched;

        private readonly BlockingCollection<RoutedMessage> _outputQueue;
        private readonly Dictionary<int, MessageConsumerDelegate> _consumerTypes;
        private readonly Dictionary<int, MessageDecoded> _listeners;

        public long MessagesDispatched => _messagesDispatched;

        public MessageDispatcher(IEnumerable<Type> consumers)
        {
            this._outputQueue = new BlockingCollection<RoutedMessage>();
            _consumerTypes = new Dictionary<int, MessageConsumerDelegate>();
            _listeners = new Dictionary<int, MessageDecoded>();

            foreach (var consumer in consumers)
            {
                if(!consumer.IsSubclassOf(typeof(NetworkMessageProcessor)))
                    throw new InvalidOperationException($"Not inherited from {nameof(NetworkMessageProcessor)}");

                var method = consumer.GetMethod(nameof(NetworkMessageProcessor.GetMessageId));

                var opCode = (int)method.Invoke(null, new object[] { });
                var consume = (MessageConsumerDelegate) Delegate.CreateDelegate(typeof(MessageConsumerDelegate),
                    consumer.GetMethod(nameof(NetworkMessageProcessor.Consume)) ??
                    throw new InvalidOperationException("Invalid Processor"), true);

                _consumerTypes.Add(opCode, consume);
            }
        }

        public unsafe void Output(AnalyzedPacket analyzedPacket)
        {
            // for default filter set we use, every single filter is mutually exclusive.
            if (analyzedPacket.Message.Length < HeaderLength) return;

            NetworkMessageHeader header;

            fixed (byte* p = &analyzedPacket.Message[0])
            {
                header = *(NetworkMessageHeader*)p;
            }

            int opcode = header.OpCode;

            if (analyzedPacket.RouteMark.GetCatalog() == MessageAttribute.CatalogLobby)
                opcode |= OPMASK_LOBBY;

            if (analyzedPacket.RouteMark.GetDirection() == MessageAttribute.DirectionSend)
                opcode = -opcode;

            if (!_consumerTypes.TryGetValue(opcode, out var consumer))
                return;

            if (!_listeners.TryGetValue(opcode, out var listener) || listener == null)
                return;

            var rm = new RoutedMessage(header, HeaderLength, analyzedPacket.Message, consumer, listener);

            EnqueueOutput(rm);
        }

        public void Start()
        {
            this._isStopping = false;

            Task.Run(() =>
            {
                Log.Warning("MessageDispatcher Output Thread Started");

                foreach (var data in this._outputQueue.GetConsumingEnumerable())
                {
                    try
                    {
                        var output = data.Consumer(data.Message, data.HeaderLength);

                        data.Listener(data.Header, output);
                        Interlocked.Increment(ref this._messagesDispatched);
                    }
                    catch(Exception e)
                    {
                        IpcClient.SendSignal(Signal.InternalExternalException, new[] {e.Message, e.StackTrace, "MessageDispatcher", "Start" });
                    }
                }

                Log.Warning("MessageDispatcher Output Thread Exited");
            });
        }

        public void Stop()
        {
            this._isStopping = true;
        }

        public void Subscribe(int opcode, MessageDecoded listener)
        {
            if (!_listeners.ContainsKey(opcode))
                _listeners.Add(opcode, null);

            if (listener != null) _listeners[opcode] += listener;
        }

        public void Unsubscribe(int opcode, MessageDecoded listener)
        {
            if (!_listeners.ContainsKey(opcode))
                return;

            if (listener != null) _listeners[opcode] -= listener;

            if (_listeners[opcode] == null) _listeners.Remove(opcode);
        }

        public void UnsubAll()
        {
            _listeners.Clear();
        }

        private void EnqueueOutput(RoutedMessage data)
        {
            if (this._isStopping)
            {
                this._outputQueue.CompleteAdding();
                return;
            }

            this._outputQueue.Add(data);
        }
    }
}
