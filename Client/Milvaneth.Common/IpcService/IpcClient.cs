using Milvaneth.Common.Logging;
using Serilog;
using System;

namespace Milvaneth.Common
{
    public static class IpcClient
    {
        public delegate void SignalOutput(int sender, Signal sig, DateTime time, string stack, string[] args);
        public static event SignalOutput OnSignalOutput;

        private static bool _isEnsured;
        private static IpcWrapper _dataBus;
        private static IpcWrapper _signalBus;

        public static long PacketSent => _dataBus?.SendSuccess ?? 0;

        public static void EnsureBus(int busId)
        {
            _dataBus = new IpcWrapper(WorkingMode.DataClient, busId);
            _signalBus = new IpcWrapper(WorkingMode.SignalClient, busId);
            _signalBus.OnReceived += GlobalSignalHandler;
            _isEnsured = true;
        }

        public static void Dispose()
        {
            _isEnsured = false;
            try { _dataBus?.Dispose(); } catch { /* ignored*/ }
            try { _signalBus?.Dispose(); } catch { /* ignored*/ }
        }

        public static void SendData(PackedResult result)
        {
            if (!_isEnsured)
                throw new InvalidOperationException("Not ensured");

            Log.Information($"Data Sent: {result.Type}");

            var dat = Serializer<PackedResult>.Serialize(result);

            _dataBus.Enqueue(dat);
        }

        public static void SendSignal(Signal sig, string[] args = null, bool noLog = false)
        {
            if (!_isEnsured) return;

            if (!noLog)
                Logger.LogSignal(sig, Environment.StackTrace, args);

            GlobalSignalRaiser(sig, DateTime.Now, Environment.StackTrace, args);
            OnSignalOutput?.Invoke(-1, sig, DateTime.Now, Environment.StackTrace, args);
        }

        private static void GlobalSignalRaiser(Signal sig, DateTime time, string stack, string[] args)
        {
            if (((int)sig & 0xFFFF_0000) == 0) return; // no internal

            var dat = Serializer<SignalPayload>.Serialize(new SignalPayload(sig, time, stack, args));

            _signalBus.Enqueue(dat);
        }

        private static void GlobalSignalHandler(byte[] data)
        {
            var dat = Serializer<SignalPayload>.Deserialize(data);
            OnSignalOutput?.Invoke(dat.Pid, dat.Sig, dat.Time, dat.Stack, dat.Args);
        }
    }
}
