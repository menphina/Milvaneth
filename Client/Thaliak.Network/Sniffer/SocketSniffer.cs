// Modifications copyright (C) 2019 Menphina

using Milvaneth.Common;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Thaliak.Network.Filter;

namespace Thaliak.Network.Sniffer
{
    public class SocketSniffer
    {
        private const int BufferSize = 1024 * 64;
        private const int MaxReceive = 100;

        private bool _isStopping;
        private bool _isPausing;
        private long _packetsObserved;
        private long _packetsCaptured;
        private Socket _socket;

        private readonly ConcurrentStack<SocketAsyncEventArgs> _receivePool;
        private readonly SemaphoreSlim _maxReceiveEnforcer = new SemaphoreSlim(MaxReceive, MaxReceive);
        private readonly BufferManager _bufferManager;
        private readonly BlockingCollection<TimestampedData> _outputQueue;
        private readonly ISnifferOutput _output;
        private Filters<IPPacket> _filters;

        public long PacketsObserved => this._packetsObserved;
        public long PacketsCaptured => this._packetsCaptured;

        public SocketSniffer(IPAddress nicAddr, Filters<IPPacket> filters, ISnifferOutput output)
        {
            this._filters = filters;
            this._outputQueue = new BlockingCollection<TimestampedData>();
            this._output = output;

            this._bufferManager = new BufferManager(BufferSize, MaxReceive);
            this._receivePool = new ConcurrentStack<SocketAsyncEventArgs>();
            var endPoint = new IPEndPoint(nicAddr, 0);

            // Capturing at the IP level is not supported on Linux
            // https://github.com/dotnet/corefx/issues/25115
            // https://github.com/dotnet/corefx/issues/30197
            var protocolType = ProtocolType.IP;

            // IPv4
            this._socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, protocolType);
            this._socket.Bind(endPoint);
            this._socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.HeaderIncluded, true);

            EnterPromiscuousMode();
        }

        private void EnterPromiscuousMode()
        {
            try
            {
                this._socket.IOControl(IOControlCode.ReceiveAll, BitConverter.GetBytes(1), new byte[4]);
            }
            catch (Exception ex)
            {
                IpcClient.SendSignal(Signal.InternalUnmanagedException,
                    new[]
                    {
                        $"Unable to enter promiscuous mode: {ex.Message} / {ex.HResult} / {Marshal.GetLastWin32Error()}",
                        "Network", "SocketSniffer", "EnterPromiscuousMode"
                    });
                IpcClient.SendSignal(Signal.MilvanethComponentExit, new[] { "Network", "Sniffer" });
                throw;
            }
        }

        public void Start()
        {
            this._isPausing = true;

            // Pre-allocate pool of SocketAsyncEventArgs for receive operations
            for (var i = 0; i < MaxReceive; i++)
            {
                var socketEventArgs = new SocketAsyncEventArgs();
                socketEventArgs.Completed += (e, args) => Receive(socketEventArgs);

                // Allocate space from the single, shared buffer
                this._bufferManager.AssignSegment(socketEventArgs);

                this._receivePool.Push(socketEventArgs);
            }

            Task.Run(() =>
            {
                Log.Warning("SocketSniffer Output Thread Started");

                // GetConsumingEnumerable() will wait when queue is empty, until CompleteAdding() is called
                foreach (var timestampedData in this._outputQueue.GetConsumingEnumerable())
                {
                    try
                    {
                        Output(timestampedData);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e, "Unhandled Error in Sniffer.Start");
                    }
                }

                Log.Warning("SocketSniffer Output Thread Exited");
            });

            Task.Run(StartReceiving);
        }

        public void Stop()
        {
            this._isStopping = true;
        }

        public void Resume(Filters<IPPacket> filters)
        {
            Update(filters);
            this._isPausing = false;
        }

        public void Pause()
        {
            this._isPausing = true;
        }

        public void Update(Filters<IPPacket> filters)
        {
            this._filters = filters;
        }

        private void EnqueueOutput(TimestampedData timestampedData)
        {
            if (this._isStopping)
            {
                this._outputQueue.CompleteAdding();
                return;
            }

            if (_isPausing) return;

            this._outputQueue.Add(timestampedData);
        }

        private void Output(TimestampedData timestampedData)
        {
            // No filter, no pass
            if (!(this._filters?.PropertyFilters?.Any() ?? false)) return;

            var packet = new IPPacket(timestampedData.Data);
            var match = this._filters.FirstMatch(packet);

            if (match == MessageAttribute.NoMatch)
            {
                return;
            }

            timestampedData.HeaderLength = packet.HeaderLength;
            timestampedData.Protocol = packet.Protocol;
            timestampedData.RouteMark = match;

            this._output.Output(timestampedData);
            Interlocked.Increment(ref this._packetsCaptured);
        }

        private void StartReceiving()
        {
            try
            {
                // Get SocketAsyncEventArgs from pool
                this._maxReceiveEnforcer.Wait();

                if (!this._receivePool.TryPop(out var socketEventArgs))
                {
                    // Because we are controlling access to pooled SocketAsyncEventArgs, this
                    // *should* never happen...
                    IpcClient.SendSignal(Signal.InternalException, new[] {"Connection pool exhausted"});
                    IpcClient.SendSignal(Signal.MilvanethComponentExit, new[] { "Network", "Sniffer" });
                    throw new Exception("Connection pool exhausted");
                }

                // Returns true if the operation will complete asynchronously, or false if it completed
                // synchronously
                var willRaiseEvent = this._socket.ReceiveAsync(socketEventArgs);

                if (!willRaiseEvent)
                {
                    Receive(socketEventArgs);
                }
            }
            catch (Exception ex)
            {
                // Exceptions while shutting down are expected
                if (!this._isStopping)
                {
                    Log.Error(ex, "Unhandled Error in Sniffer.StartReceiving");
                }

                this._socket.Close();
                this._socket = null;
            }
        }

        private void Receive(SocketAsyncEventArgs e)
        {
            // Start a new receive operation straight away, without waiting
            StartReceiving();

            try
            {
                if (e.SocketError != SocketError.Success)
                {
                    if (!this._isStopping)
                    {
                        IpcClient.SendSignal(Signal.InternalUnmanagedException,
                            new[] {$"Socket error: \n{(int) e.SocketError}", "Network", "SocketSniffer", "Receive"});
                    }

                    return;
                }

                if (e.BytesTransferred <= 0)
                {
                    return;
                }

                Interlocked.Increment(ref this._packetsObserved);

                // Copy the bytes received into a new buffer
                var buffer = new byte[e.BytesTransferred];
                Buffer.BlockCopy(e.Buffer, e.Offset, buffer, 0, e.BytesTransferred);

                EnqueueOutput(new TimestampedData(DateTime.UtcNow, buffer));
            }
            catch (SocketException ex)
            {
                IpcClient.SendSignal(Signal.InternalUnmanagedException,
                    new[]
                    {
                        $"Socket exception: {ex.Message} / {ex.HResult} / {Marshal.GetLastWin32Error()}", "Network",
                        "SocketSniffer", "Receive"
                    });
            }
            catch (Exception ex)
            {
                IpcClient.SendSignal(Signal.InternalException,
                    new[] {$"Error: {ex.Message}", "Network", "SocketSniffer", "Receive"});
            }
            finally
            {
                // Put the SocketAsyncEventArgs back into the pool
                if (!this._isStopping && this._socket != null && this._socket.IsBound)
                {
                    this._receivePool.Push(e);
                    this._maxReceiveEnforcer.Release();
                }
            }
        }
    }
}
