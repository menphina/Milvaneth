using Serilog;

namespace Milvaneth.Service
{
    internal static class LoggingManagementService
    {
        public delegate void LogOutputDelegate(string data);

        public static LogOutputDelegate OnNewLogLine = Log.Information;

        public static void Initialize()
        {
            LoggingManagementService.WriteLine($"{nameof(LoggingManagementService)} initialized", "LogMgmt");
        }

        public static void Dispose() { }

        public static void WriteLine(string message, string sender)
        {
            OnNewLogLine?.Invoke($"<{sender}> {message}");
        }

        public static void WriteLine(string message, int pid)
        {
            OnNewLogLine?.Invoke($"<PID {pid}> {message}");
        }
    }
}
