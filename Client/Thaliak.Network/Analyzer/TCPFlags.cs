using System;

namespace Thaliak.Network.Analyzer
{
    [Flags]
    public enum TCPFlags
    {
        NONE = 0,
        FIN = 1,
        SYN = 2,
        RST = 4,
        PSH = 8,
        ACK = 16,
        URG = 32,
        ECE = 64,
        CWR = 128,
        NS = 256,
    }
}
