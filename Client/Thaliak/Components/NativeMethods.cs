using Milvaneth.Common;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

namespace Thaliak.Components
{
    internal static class NativeMethods
    {
        private const ProcessAccessFlags ProcessFlags =
            ProcessAccessFlags.VirtualMemoryRead
            | ProcessAccessFlags.VirtualMemoryWrite
            | ProcessAccessFlags.VirtualMemoryOperation
            | ProcessAccessFlags.QueryInformation;

        internal static IntPtr OpenProc(int pid)
        {
            var ptr = OpenProcess(ProcessFlags, false, pid);
            if (ptr != IntPtr.Zero) return ptr;

            IpcClient.SendSignal(Signal.InternalException, new []{ "Could not open handle: " + Marshal.GetLastWin32Error(), "MemoryCore", "OpenProc"});
            throw new Exception("Could not open handle: " + Marshal.GetLastWin32Error());
        }

        internal static void CloseProc(IntPtr ptr)
        {
            if (CloseHandle(ptr)) return;

            IpcClient.SendSignal(Signal.InternalException, new[] { "Could not close handle: " + Marshal.GetLastWin32Error(), "MemoryCore", "CloseProc" });
            throw new Exception("Could not close handle: " + Marshal.GetLastWin32Error());
        }

        internal static int Write(IntPtr handle, IntPtr address, byte[] buffer)
        {
            if (WriteProcessMemory(handle, address, buffer, buffer.Length, out _)) return buffer.Length;

            IpcClient.SendSignal(Signal.InternalException, new[] { "Could not write process memory: " + Marshal.GetLastWin32Error(), "MemoryCore", "Write" });
            throw new Exception("Could not write process memory: " + Marshal.GetLastWin32Error());
        }

        internal static byte[] Read(IntPtr handle, IntPtr address, long size)
        {
            var buffer = new byte[size];
            if (ReadProcessMemory(handle, address, buffer, buffer.Length, out _)) return buffer;

            IpcClient.SendSignal(Signal.InternalException, new[] { "Could not read process memory: " + Marshal.GetLastWin32Error(), "MemoryCore", "Read" });
            throw new Exception("Could not read process memory: " + Marshal.GetLastWin32Error());

        }

        internal static IntPtr TraceTree(IntPtr handle, IntPtr baseAddr, int[] offsets, int finalOffset)
        {
#if x64
            const int size = 8;
#elif x86
            const int size = 4;
#endif // Arch
            var buffer = new byte[size];
            foreach (var offset in offsets)
            {
                if (!ReadProcessMemory(handle, IntPtr.Add(baseAddr, offset), buffer, size, out _))
                {
                    IpcClient.SendSignal(Signal.InternalException, new[] { "Unable to calculate address: " + Marshal.GetLastWin32Error(), "MemoryCore", "TraceTree" });
                    throw new Exception("Unable to calculate address: " + Marshal.GetLastWin32Error());
                }
#if x64
                baseAddr = new IntPtr(BitConverter.ToInt64(buffer, 0));
#elif x86
                baseAddr = new IntPtr(BitConverter.ToInt32(buffer, 0));
#endif // Arch
                if (baseAddr == IntPtr.Zero) return IntPtr.Zero;
            }

            return IntPtr.Add(baseAddr, finalOffset);
        }

        internal static IntPtr GetThreadStack(IntPtr processHandle, Process process, int threadNr)
        {
            return IntPtr.Zero;
        }

        #region NativeMethods

        [SuppressUnmanagedCodeSecurity] // high invoke frequency and relatively safe
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer,
            int dwSize, out IntPtr lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize,
            out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(ProcessAccessFlags processAccess, bool bInheritHandle, int processId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [Flags]
        private enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }

        #endregion
    }
}
