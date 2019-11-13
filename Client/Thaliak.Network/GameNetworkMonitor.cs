using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Thaliak.Network.Analyzer;
using Thaliak.Network.Dispatcher;
using Thaliak.Network.Filter;
using Thaliak.Network.Sniffer;
using Thaliak.Network.Utilities;

namespace Thaliak.Network
{
    // This is the 'default' entry class of Thaliak.Network, which bundles three sub-components
    // We use StaticallyLinkedMessageDispatcher to improve performance.
    public class GameNetworkMonitor : IDisposable
    {
        public Process ProcessWorking { get; }
        public IEnumerable<Type> ConsumerSet { get; }
        public long PacketsObserved => _sniffer?.PacketsObserved ?? -1;
        public long PacketsCaptured => _sniffer?.PacketsCaptured ?? -1;
        public long PacketsAnalyzed => _analyzer.PacketsAnalyzed;
        public long MessagesProcessed => _analyzer.MessagesProcessed;
        public long MessagesDispatched => _dispatcher.MessagesDispatched;

        private readonly MessageDispatcher _dispatcher;
        private readonly PacketAnalyzer _analyzer;
        private SocketSniffer _sniffer;
        private IPAddress _interfaceAddress;

        public GameNetworkMonitor(Process p)
        {
            this.ProcessWorking = p;

            var consumers = ConsumerSearcher.FindConsumers("Thaliak.Network.Messages");

            ConsumerSet = consumers;

            _dispatcher = new MessageDispatcher(ConsumerSet);
            _analyzer = new PacketAnalyzer(_dispatcher);
        }

        public void Start()
        {
            var conn = ConnectionPicker.GetGameConnections(ProcessWorking);
            var filters = FilterBuilder.BuildDefaultFilter(conn);
            var nic = NetworkInterfaceInfo.GetDefaultInterface();

            Start(nic.IPAddress, filters);
        }

        public void Start(IPAddress nicAddress, Filters<IPPacket> filters)
        {
            if (_sniffer == null || !nicAddress.Equals(_interfaceAddress))
            {
                RecycleSniffer(nicAddress, filters);
                _interfaceAddress = nicAddress;
            }

            _dispatcher.Start();
            _analyzer.Start();
            _sniffer.Resume(filters);
        }
        
        public void Stop()
        {
            _sniffer.Pause();
            _analyzer.Stop();
            _dispatcher.Stop();
        }

        public void Update(IPAddress nicAddress, Filters<IPPacket> filters)
        {
            if (_sniffer == null || !nicAddress.Equals(_interfaceAddress))
            {
                RecycleSniffer(nicAddress, filters);
                _interfaceAddress = nicAddress;
            }

            _sniffer.Update(filters);
        }

        public void Dispose()
        {
            _sniffer.Stop();
            _analyzer.Stop();
            _dispatcher.Stop();
            _analyzer.Dispose();
        }

        public void Subscribe(int opcode, MessageDecoded listener)
        {
            _dispatcher.Subscribe(opcode, listener);
        }

        public void Unsubscribe(int opcode, MessageDecoded listener)
        {
            _dispatcher.Unsubscribe(opcode, listener);
        }

        public void UnsubAll()
        {
            _dispatcher.UnsubAll();
        }

        private void RecycleSniffer(IPAddress nicAddress, Filters<IPPacket> filters)
        {
            if (_sniffer != null)
            {
                _sniffer.Pause();
                _sniffer.Stop();
                _sniffer = null;
            }

            _sniffer = new SocketSniffer(nicAddress, filters, _analyzer);
            _sniffer.Start();
            _sniffer.Resume(filters);
        }
    }
}
