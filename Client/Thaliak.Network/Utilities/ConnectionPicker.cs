using Milvaneth.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Thaliak.Network.Utilities
{
    public class ConnectionPicker
    {
        public static List<Connection> GetGameConnections(Process process)
        {
            var currentConnections = GetConnections(process);
            var lobbyEndPoint = GetLobbyEndPoint(process);

            return currentConnections.Where(x => !x.RemoteEndPoint.Equals(lobbyEndPoint)).ToList();
        }

        public static IPEndPoint GetLobbyEndPoint(Process process)
        {
            string lobbyHost = null;
            var lobbyPort = 0;

            try
            {
                using (var searcher =
                    new ManagementObjectSearcher(
                        "SELECT CommandLine FROM Win32_Process WHERE ProcessId = " + process.Id))
                {
                    foreach (var @object in searcher.Get())
                    {
                        var commandline = @object["CommandLine"].ToString();
                        var argv = commandline.Split(' ');

                        foreach (var arg in argv)
                        {
                            var parts = arg.Split('=');

                            if (parts.Length != 2) continue;

                            if (parts[0].Contains("LobbyHost01"))
                            {
                                lobbyHost = parts[1];
                            }
                            else if (parts[0].Contains("LobbyPort01"))
                            {
                                lobbyPort = int.Parse(parts[1]);
                            }
                        }
                    }
                }

                if (lobbyHost == null || lobbyPort <= 0) return null;
            }
            catch (Exception e)
            {
                IpcClient.SendSignal(Signal.InternalException, new []{$"GetLobbyEndPoint Failed in Connection Picker: \n{e.Message}", "Network", "ConnectionPicker", "GetLobbyEndPoint" });
                return null;
            }

            var address = Dns.GetHostAddresses(lobbyHost)[0];

            return new IPEndPoint(address, lobbyPort);
        }

        public static List<Connection> GetConnections(Process process)
        {
            var connections = new List<Connection>();

            var tcpTable = IntPtr.Zero;
            var tcpTableLength = 0;

            if (NativeMethods.GetExtendedTcpTable(tcpTable, ref tcpTableLength, false, AddressFamily.InterNetwork,
                    NativeMethods.TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_CONNECTIONS) == 0) return connections;

            try
            {
                tcpTable = Marshal.AllocHGlobal(tcpTableLength);
                if (NativeMethods.GetExtendedTcpTable(tcpTable, ref tcpTableLength, false, AddressFamily.InterNetwork,
                        NativeMethods.TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_CONNECTIONS) == 0)
                {
                    var table = (NativeMethods.TcpTable) Marshal.PtrToStructure(tcpTable,
                        typeof(NativeMethods.TcpTable));

                    var rowPtr = new IntPtr(tcpTable.ToInt64() + Marshal.SizeOf(typeof(uint)));
                    for (var i = 0; i < table.length; i++)
                    {
                        var row = (NativeMethods.TcpRow) Marshal.PtrToStructure(rowPtr, typeof(NativeMethods.TcpRow));

                        if (row.owningPid == process.Id)
                        {
                            var local = new IPEndPoint(row.localAddr,
                                (ushort) IPAddress.NetworkToHostOrder((short) row.localPort));
                            var remote = new IPEndPoint(row.remoteAddr,
                                (ushort) IPAddress.NetworkToHostOrder((short) row.remotePort));
                            connections.Add(new Connection(local, remote));
                        }

                        rowPtr = new IntPtr(rowPtr.ToInt64() + Marshal.SizeOf(typeof(NativeMethods.TcpRow)));
                    }
                }
                else
                {
                    IpcClient.SendSignal(Signal.InternalUnmanagedException,
                        new []{$"GetExtendedTcpTable Failed in Connection Picker: {Marshal.GetLastWin32Error()}", "Network", "ConnectionPicker", "GetConnections" });
                    if(connections.Count == 0)
                        IpcClient.SendSignal(Signal.MilvanethComponentExit, new[] { "Network", "ConnectionPicker" });
                }
            }
            catch (Exception ex)
            {
                IpcClient.SendSignal(Signal.InternalException, new []{ex.Message, "Network", "ConnectionPicker", "GetConnections" });
                if (connections.Count == 0)
                    IpcClient.SendSignal(Signal.MilvanethComponentExit, new[] { "Network", "ConnectionPicker" });
            }
            finally
            {
                if (tcpTable != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(tcpTable);
                }
            }

            return connections;
        }

        #region P/Invoke

        private class NativeMethods
        {
            [DllImport("iphlpapi.dll", SetLastError = true)]
            public static extern uint GetExtendedTcpTable(IntPtr pTcpTable, ref int dwOutBufLen, bool sort,
                AddressFamily ipVersion, TCP_TABLE_CLASS tblClass, uint reserved = 0);

            public enum TCP_TABLE_CLASS
            {
                TCP_TABLE_BASIC_LISTENER = 0,
                TCP_TABLE_BASIC_CONNECTIONS = 1,
                TCP_TABLE_BASIC_ALL = 2,
                TCP_TABLE_OWNER_PID_LISTENER = 3,
                TCP_TABLE_OWNER_PID_CONNECTIONS = 4,
                TCP_TABLE_OWNER_PID_ALL = 5,
                TCP_TABLE_OWNER_MODULE_LISTENER = 6,
                TCP_TABLE_OWNER_MODULE_CONNECTIONS = 7,
                TCP_TABLE_OWNER_MODULE_ALL = 8
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct TcpTable
            {
                public uint length;
                public TcpRow row;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct TcpRow
            {
                public TcpState state;
                public uint localAddr;
                public uint localPort;
                public uint remoteAddr;
                public uint remotePort;
                public uint owningPid;
            }
        }

        #endregion
    }
}
