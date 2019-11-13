using Serilog;
using Serilog.Events;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Milvaneth.Common.Logging
{
    public static class Logger
    {
        private static LogOutput _output;
        public static ILogger Worker => _output?.Logger;

        public static void Initialize(bool verbose, bool logChat)
        {
            if(_output != null)
                throw new InvalidOperationException("A logger has already been created");

            _output = new LogOutput(verbose ? LogEventLevel.Verbose : LogEventLevel.Information, logChat);
        }

        public static void LogSignal(Signal sig, string stack, string[] args) => _output?.LogSignal(sig, stack, args);
        public static void LogChat(string chat) => _output?.LogChat(chat);

        private class LogOutput
        {
            private readonly int _pid;
            private readonly bool _logChat;
            private readonly string _logPath;

            public readonly ILogger Logger;

            public LogOutput(LogEventLevel level, bool logChat)
            {
                _logChat = logChat;
                _pid = Process.GetCurrentProcess().Id;
                _logPath = Helper.GetMilFilePathRaw(Path.Combine("log", $"{_pid}-.log"));

                Directory.CreateDirectory(Helper.GetMilFilePathRaw("log"));

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Is(level)
                    .WriteTo.Console()
                    .WriteTo.File(_logPath, rollingInterval: RollingInterval.Day, outputTemplate: $"{{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}} [{{Level:u3}} @ {_pid}] {{Message:lj}}{{NewLine}}{{Exception}}")
                    .CreateLogger();

                Logger = Log.Logger;
            }

            public void LogSignal(Signal sig, string stack, string[] args)
            {
                try
                {
                    var signal = Enum.GetName(typeof(Signal), sig);
                    if (string.IsNullOrEmpty(signal))
                        signal = $"SIGINT{(int)sig}";

                    var sb = new StringBuilder();
                    var counter = 0;
                    sb.AppendLine($"Signal: {signal} from {_pid}");
                    sb.AppendLine("Stacktrace:");
                    sb.AppendLine(stack);
                    sb.AppendLine("Arguments:");
                    if (args == null)
                    {
                        sb.AppendLine($"   [NO ARG] = NULL");
                    }
                    else
                    {
                        foreach (var i in args)
                        {
                            sb.AppendLine($"   [{counter++}] = {i}");
                        }
                    }

                    Log.Error(sb.ToString());
                }
                catch
                {
                    // ignored
                }
            }

            public void LogChat(string chat)
            {
                if (!_logChat || string.IsNullOrWhiteSpace(chat))
                    return;

                Logger.Information("In-game Chat: " + chat);
            }
        }
    }
}
