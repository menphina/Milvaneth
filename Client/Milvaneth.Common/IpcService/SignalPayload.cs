using System;
using System.Diagnostics;
using MessagePack;

namespace Milvaneth.Common
{
    [MessagePackObject]
    public class SignalPayload
    {
        [Key(0)]
        public int Pid { get; set; }
        [Key(1)]
        public Signal Sig { get; set; }
        [Key(2)]
        public DateTime Time { get; set; }
        [Key(3)]
        public string Stack { get; set; }
        [Key(4)]
        public string[] Args { get; set; }
        [IgnoreMember]
        private static int _pid;

        public SignalPayload(Signal sig, DateTime time, string stack, string[] args)
        {
            if (_pid == 0)
            {
                _pid = Process.GetCurrentProcess().Id;
            }

            Pid = _pid;
            Sig = sig;
            Time = time;
            Stack = stack;
            Args = args;
        }

        [SerializationConstructor]
        public SignalPayload(int pid, Signal sig, DateTime time, string stack, string[] args)
        {
            Pid = pid;
            Sig = sig;
            Time = time;
            Stack = stack;
            Args = args;
        }
    }
}
