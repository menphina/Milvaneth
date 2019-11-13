using System;
using System.Diagnostics;
using Milvaneth.Common;

namespace Thaliak.Components
{
    internal sealed class MemoryService : IDisposable
    {
        internal MemoryInfo Info;

        internal MemoryService(Process process)
        {
            if (process == null || process.HasExited || string.IsNullOrEmpty(process.GetMainModuleFileName()))
                throw new ArgumentException("Invalid process");

            Info = new MemoryInfo
            {
                ProcHandle = NativeMethods.OpenProc(process.Id),
                BaseAddr = process.MainModule.BaseAddress,
                Process = process
            };
        }

        public void Dispose()
        {
            NativeMethods.CloseProc(Info.ProcHandle);
            Info = null;
        }

        internal IntPtr TraceTree(IntPtr entry, int[] offsets, int finalOffset, bool noAddBase)
        {
            IntPtr pointer;
            if (noAddBase)
                pointer = entry;                
            else
                pointer = new IntPtr(entry.ToInt64() + Info.BaseAddr.ToInt64());

            if (offsets.Length == 0)
                return pointer + finalOffset;
            return NativeMethods.TraceTree(Info.ProcHandle, pointer, offsets, finalOffset);
        }

        internal byte[] Read(IntPtr address, long size)
        {
            return NativeMethods.Read(Info.ProcHandle, address, size);
        }

#if EnableMemoryWrite
        internal int Write(IntPtr address, byte[] data)
        {
            return NativeMethods.Write(Info.ProcHandle, address, data);
        }
#endif

        internal IntPtr GetThreadStack(int threadNr)
        {
            if (threadNr < 0)
                throw new ArgumentException("Invalid thread scan request");

            var ret = NativeMethods.GetThreadStack(Info.ProcHandle, Info.Process, threadNr);
            return ret != IntPtr.Zero ? ret : throw new ApplicationException("Could not find thread stack");
        }

        internal class MemoryInfo
        {
            internal IntPtr ProcHandle;
            internal IntPtr BaseAddr;
            internal Process Process;
        }
    }
}