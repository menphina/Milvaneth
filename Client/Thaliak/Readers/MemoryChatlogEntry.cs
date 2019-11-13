using System;
using System.Runtime.InteropServices;

namespace Thaliak.Readers
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct MemoryChatlogEntry
    {
        [FieldOffset(0)] internal IntPtr LengthStart;
        [FieldOffset(8)] internal IntPtr LengthEnd;
        // +16 LengthCurrent
        [FieldOffset(24)] internal IntPtr LogStart;
        [FieldOffset(32)] internal IntPtr LogEnd;
        // +40 LogCurrent
    }
}