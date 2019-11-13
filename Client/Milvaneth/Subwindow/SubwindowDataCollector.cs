using System;
using Milvaneth.Common;
using Milvaneth.Service;
using System.Threading;

namespace Milvaneth.Subwindow
{
    internal static class SubwindowDataCollector
    {
        private static readonly object LockObject = new object();
        private static bool _filterOn;
        private static LobbyServiceResult _service;
        private static LobbyCharacterResult _character;
        private static long _serviceTime;
        private static long _characterTime;

        public static bool Collect(int timeout, out LobbyServiceResult service, out LobbyCharacterResult character)
        {
            service = null;
            character = null;

            SubprocessManagementService.SpawnAll(out _);
            SubprocessManagementService.SpawnHunter();

            lock (LockObject)
            {
                if (!_filterOn)
                {
                    TransmittingManagementService.OnDataOutput += Filter;
                    _filterOn = true;
                }
            }

            int cycle = 0;
            while (Math.Abs(_serviceTime - Environment.TickCount) > 10000 || Math.Abs(_characterTime - Environment.TickCount) > 10000)
            {
                if (100 * cycle > timeout)
                {
                    return false;
                }

                Thread.Sleep(100);
                cycle++;
            }

            service = _service;
            character = _character;

            lock (LockObject)
            {
                if (_filterOn)
                {
                    TransmittingManagementService.OnDataOutput -= Filter;
                    _filterOn = false;
                }
            }

            return true;
        }

        private static void Filter(int gameId, PackedResult data)
        {
            if (data.Type == PackedResultType.LobbyService && data.Result is LobbyServiceResult svc)
            {
                _service = svc;
                _serviceTime = Environment.TickCount;
            }

            if (data.Type == PackedResultType.LobbyCharacter && data.Result is LobbyCharacterResult chr)
            {
                _character = chr;
                _characterTime = Environment.TickCount;
            }
        }
    }
}
