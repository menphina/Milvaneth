using Milvaneth.Common.Logging;
using System;
using System.Collections.Generic;

namespace Milvaneth.Common
{
    public class IpcServer : IDisposable
    {
        public delegate void DataOutput(int busId, PackedResult data);
        public delegate void SignalOutput(int sender, Signal sig, DateTime time, string stack, string[] args);

        public DataOutput OnDataOutput;
        public SignalOutput OnSignalOutput;

        private Dictionary<int, BusTuple> _busDictionary;
        public IpcServer()
        {
            _busDictionary = new Dictionary<int, BusTuple>();
        }

        public void Dispose()
        {
            foreach (var i in _busDictionary)
            {
                try { i.Value.Dispose(); } catch { /* ignored */ }
            }

            _busDictionary = null;
        }

        public void AddBus(int busId)
        {
            _busDictionary.Add(busId, new BusTuple(busId, dat => OnDatRecv(busId, dat), OnSigRecv));
        }

        public void RemoveBus(int busId)
        {
            _busDictionary[busId]?.Dispose();
            _busDictionary.Remove(busId);
        }

        public void SendSignal(Signal sig, string[] args = null, bool noLog = false)
        {
            if (!noLog)
                Logger.LogSignal(sig, Environment.StackTrace, args);

            GlobalSignalRaiser(sig, DateTime.Now, Environment.StackTrace, args);
            OnSignalOutput?.Invoke(-1, sig, DateTime.Now, Environment.StackTrace, args);
        }

        private void GlobalSignalRaiser(Signal sig, DateTime time, string stack, string[] args)
        {
            if (((int)sig & 0xFFFF_0000) == 0) return; // no internal

            var dat = Serializer<SignalPayload>.Serialize(new SignalPayload(sig, time, stack, args));

            foreach (var k in _busDictionary)
            {
                k.Value.Signal.Enqueue(dat);
            }
        }

        private void OnDatRecv(int id, byte[] msg)
        {
            var dat = Serializer<PackedResult>.Deserialize(msg);
            OnDataOutput?.Invoke(id, dat);
        }

        private void OnSigRecv(byte[] msg)
        {
            var dat = Serializer<SignalPayload>.Deserialize(msg);
            OnSignalOutput?.Invoke(dat.Pid, dat.Sig, dat.Time, dat.Stack, dat.Args);
        }

        private class BusTuple : IDisposable
        {
            public BusTuple(int busId, OnDataReceivedDelegate datRecv, OnDataReceivedDelegate sigRecv)
            {
                Data = new IpcWrapper(WorkingMode.DataServer, busId);
                Signal = new IpcWrapper(WorkingMode.SignalServer, busId);
                Data.OnReceived += datRecv;
                Signal.OnReceived += sigRecv;
            }

            public IpcWrapper Data;
            public IpcWrapper Signal;

            public void Dispose()
            {
                try { Data?.Dispose(); } catch { /* ignored*/ }
                try { Signal?.Dispose(); } catch { /* ignored*/ }

                Data = null;
                Signal = null;
            }
        }
    }
}