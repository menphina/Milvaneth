using Milvaneth.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Thaliak.Network.Utilities;

namespace Milvaneth.Cmd
{
    internal class GameStatusManager
    {
        public delegate void OnStatusEventDelegate();
        public delegate long OnGetPacketAmountDelegate();

        public OnStatusEventDelegate OnGlobalStart;
        public OnStatusEventDelegate OnGlobalStop;
        public OnStatusEventDelegate OnWorldStart;
        public OnStatusEventDelegate OnWorldStop;
        public OnStatusEventDelegate OnLobbyUpdate;
        public OnGetPacketAmountDelegate GetPacketAmount;

        public Process GameProcess { get; private set; }
        public List<Connection> GameConnection { get; private set; }
        public List<Connection> LobbyConnection { get; private set; }
        public bool Stopping { get; private set; }

        private NetworkState _networkState = NetworkState.Lobby;
        private ManagementEventWatcher _startWatch;
        private ManagementEventWatcher _stopWatch;
        private readonly int _parentPid;
        private int _gamePid;
        private int _packetCycle;
        private int _noPackCycle;
        private int _softStopCycle;
        private long _previousGamePacketCount;
        private bool _requireSoftStop;
        private IPEndPoint _lobbyEndPoint;
        private ConnectionEqualityComparer _connEqCp;
        private MultiSetComparer<Connection> _connSetEqCp;

        public GameStatusManager(int gamePid, int parentPid)
        {
            _gamePid = gamePid;
            _parentPid = parentPid;
            _previousGamePacketCount = 0;

            RegisterProcessEvent(gamePid < 0);
        }

        public void WaitForProcess()
        {
            while (_gamePid < 0)
            {
                Thread.Sleep(500);
                if (Stopping)
                    throw new OperationCanceledException("Parent exited while hunting");
            }

            var gp = Process.GetProcessById(_gamePid);
            if (!Helper.IsChineseVersion(gp))
            {
                Log.Error("Game process not Chinese version");
                IpcClient.SendSignal(Signal.ClientProcessDown, new[] { _gamePid.ToString() });
                IpcClient.SendSignal(Signal.MilvanethComponentExit, new[] { "GameStatusManager", ".ctor" });
                throw new InvalidOperationException("Target process is not a FFXIV DX11 process");
            }

            if (!SystemInformation.IsAdmin())
            {
                Log.Error("Insufficient Privilege");
                IpcClient.SendSignal(Signal.ClientInsuffcientPrivilege, new[] { _gamePid.ToString() });
                IpcClient.SendSignal(Signal.MilvanethComponentExit, new[] { "GameStatusManager", ".ctor" });
                throw new InvalidOperationException("Insufficient Privilege");
            }

            GameProcess = gp;
            GameConnection = new List<Connection>();
            LobbyConnection = new List<Connection>();
            _connEqCp = new ConnectionEqualityComparer();
            _connSetEqCp = new MultiSetComparer<Connection>(_connEqCp);
            _lobbyEndPoint = ConnectionPicker.GetLobbyEndPoint(GameProcess);
        }

        public void StartMonitoringService()
        {
            Task.Run(() =>
            {
                Log.Warning("GameStatusManager Output Thread Started");

                OnGlobalStart?.Invoke();

                while (!Stopping)
                {
                    NetworkStateMachine();

                    Thread.Sleep(500);
                }

                OnGlobalStop?.Invoke();

                Log.Warning("GameStatusManager Output Thread Exited");
            });
        }

        public void Dispose()
        {
            RequireGlobalStop();

            try { _startWatch?.Dispose(); } catch { /* ignored */ }
            try { _stopWatch?.Dispose(); } catch { /* ignored */ }
            try { GameProcess?.Dispose(); } catch { /* ignored */ }

            _stopWatch = null;
            GameProcess = null;
        }

        public void SuspectNetworkStop()
        {
            Interlocked.Exchange(ref _softStopCycle, 0);
            _requireSoftStop = true;
        }

        public void RequireGlobalStop()
        {
            Stopping = true;
        }

        private void RegisterProcessEvent(bool registerStart)
        {
            if (registerStart)
            {
                _startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
                _startWatch.EventArrived += HasProcStart;
                _startWatch.Start();
                Log.Information("Hunting Mode Configured. Waiting for new FFXIV DX11 instance.");
            }

            _stopWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace"));
            _stopWatch.EventArrived += HasProcStop;
            _stopWatch.Start();
        }

        private void HasProcStart(object sender, EventArrivedEventArgs e)
        {
            var starting = (uint) e.NewEvent.Properties["ProcessID"].Value;
            var procname = (string) e.NewEvent.Properties["ProcessName"].Value;

            if (procname != "ffxiv_dx11.exe" || !Helper.IsChineseVersion(Process.GetProcessById((int)starting))) return;

            Log.Information($"Hunted Process: {starting}");
            _gamePid = (int)starting;
            _startWatch.Stop();
        }

        private void HasProcStop(object sender, EventArrivedEventArgs e)
        {
            var exiting = (uint) e.NewEvent.Properties["ProcessID"].Value;

            if (exiting != _gamePid && exiting != _parentPid) return;

            Log.Information($"Exiting Process: {exiting}");
            IpcClient.SendSignal(Signal.ClientProcessDown, new[] { GameProcess?.Id.ToString() ?? "-1" });
            Stopping = true;
        }

        private void NetworkStateMachine()
        {
            const int gameConnectionCount = 2;

            switch (_networkState)
            {
                case NetworkState.Lobby:
                    _packetCycle = 0;
                    _noPackCycle = 0;
                    _requireSoftStop = false;
                    Interlocked.Exchange(ref _softStopCycle, 0);

                    var l_conn = ConnectionPicker.GetConnections(GameProcess);
                    if (!l_conn.Any())
                    {
                        _networkState = NetworkState.Unconnected;
                        LobbyConnection.Clear();
                        Log.Information($"Network State: NoConnection");
                        break;
                    }

                    LobbyConnection = ExtractConnections(l_conn, null);
                    OnLobbyUpdate?.Invoke();

                    var l_game = ExtractConnections(l_conn, LobbyConnection);
                    if (l_game.Count == gameConnectionCount)
                    {
                        GameConnection = l_game;
                        _networkState = NetworkState.GameUninitialized;
                        Log.Information($"Network State: GameConnDetected");
                    }

                    break;

                case NetworkState.Unconnected:
                    _networkState = NetworkState.Fail;

                    if (!NetworkInterface.GetIsNetworkAvailable()) break;
                    if (!PingHost(ConnectionPicker.GetLobbyEndPoint(GameProcess))) break;

                    _networkState = NetworkState.Pending;
                    Log.Information($"Network State: WaitingLogin");

                    break;

                case NetworkState.Fail:
                    Log.Information($"Network State: FailNetworkCheck");
                    IpcClient.SendSignal(Signal.ClientNetworkFail, new[] { GameProcess.Id.ToString() });
                    Stopping = true;

                    break;

                case NetworkState.Pending:
                    if (ConnectionPicker.GetConnections(GameProcess).Any())
                    {
                        _networkState = NetworkState.Lobby;
                        Log.Information($"Network State: ConnectedToLobby");
                    }

                    break;

                case NetworkState.GameUninitialized:
                    OnWorldStart?.Invoke();
                    _networkState = NetworkState.GameInitialized;
                    Log.Information($"Network State: InitializeInvoked");

                    break;

                case NetworkState.GameInitialized:
                    if (_requireSoftStop)
                    {
                        Interlocked.Increment(ref _softStopCycle);
                        _networkState = NetworkState.GameNoPacket;
                        Log.Information($"Network State: ConnectivityCheck");
                        break;
                    }

                    if (GetPacketAmount != null && _previousGamePacketCount != GetPacketAmount?.Invoke())
                    {
                        _noPackCycle = 0;
                        _previousGamePacketCount = GetPacketAmount.Invoke();
                        break;
                    }

                    _packetCycle++;
                    _noPackCycle++;

                    if (_packetCycle > 240) // 2min, Quality assurance
                    {
                        _networkState = NetworkState.GameNoPacket;
                        Log.Information($"Network State: ScheduledQA");
                    }

                    if(_noPackCycle > 10) // 5s, Suspected disconnected
                    {
                        _networkState = NetworkState.GameNoPacket;
                        Log.Information($"Network State: NoPackNetworkCheck");
                    }

                    break;

                case NetworkState.GameNoPacket:
                    var gnp_conn = ConnectionPicker.GetConnections(GameProcess);

                    if (!_connSetEqCp.Equals(gnp_conn, GameConnection))
                    {
                        OnWorldStop?.Invoke();
                        _networkState = NetworkState.Lobby;
                        Log.Information($"Network State: ConnectionLost");
                        break;
                    }

                    if (_softStopCycle > 20) // 10s, Kept monitoring
                    {
                        _requireSoftStop = false;
                        Interlocked.Exchange(ref _softStopCycle, 0);
                    }

                    _packetCycle = 0;
                    _noPackCycle = 0;
                    _networkState = NetworkState.GameInitialized;
                    Log.Information($"Network State: Normal");

                    break;

                default:
                    _networkState = NetworkState.Lobby;
                    Log.Information($"Network State: UnrecognizedToLobby");

                    break;
            }
        }

        public static bool PingHost(IPEndPoint endPoint)
        {
            var address = endPoint.Address;
            var pingOptions = new PingOptions(128, true);
            var ping = new Ping();
            var buffer = new byte[32];
            var succ = 0;

            for (var i = 0; i < 4; i++)
            {
                try
                {
                    var pingReply = ping.Send(address, 1000, buffer, pingOptions);

                    if (pingReply != null && pingReply.Status == IPStatus.Success)
                    {
                        succ++;
                    }
                }
                catch
                {
                    return false;
                }
                
            }

            return succ > 2;
        }

        private List<Connection> ExtractConnections(List<Connection> connList, List<Connection> connLobby)
        {
            // remove api.github.com & ocsp.digicert.com
            var connAvailable = connList.Where(x => x.RemoteEndPoint.Port != 443 && x.RemoteEndPoint.Port != 80).ToList();

            if (connLobby != null)
                return connAvailable.Where(x => !connLobby.Contains(x, _connEqCp)).ToList();
            
            var connCandidate = connAvailable.Where(x => x.RemoteEndPoint.Equals(_lobbyEndPoint)).ToList();
            if (connCandidate.Count == 0 && connAvailable.Count == 1) // a dirty way to get lobby connection when game is proxied
                connCandidate = new List<Connection>(connAvailable);
            return connCandidate;
        }

        private enum NetworkState
        {
            Unconnected,
            Pending,
            Fail,
            Lobby,
            GameUninitialized,
            GameInitialized,
            GameNoPacket,
        }
    }
}
