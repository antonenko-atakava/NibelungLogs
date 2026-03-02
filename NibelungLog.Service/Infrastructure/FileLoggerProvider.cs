using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;

namespace NibelungLog.Service.Infrastructure;

public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly string _logFilePath;
    private readonly StreamWriter _writer;

    public FileLoggerProvider(string logFilePath)
    {
        var logDirectory = Path.GetDirectoryName(logFilePath);
        if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            Directory.CreateDirectory(logDirectory);

        _logFilePath = logFilePath;
        var fileStream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
        _writer = new StreamWriter(fileStream, Encoding.UTF8)
        {
            AutoFlush = true
        };
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FileLogger(_writer);
    }

    public void Dispose()
    {
        _writer?.Dispose();
    }

    private sealed class FileLogger : ILogger
    {
        private readonly StreamWriter _writer;

        public FileLogger(StreamWriter writer)
        {
            _writer = writer;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Information;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var level = GetLogLevelString(logLevel);
            
            var logLine = $"{timestamp} [{level}] {message}";
            
            if (exception != null)
                logLine += $"\n{exception}";

            lock (_writer)
            {
                _writer.WriteLine(logLine);
            }
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => "TRACE",
                LogLevel.Debug => "DEBUG",
                LogLevel.Information => "INFO ",
                LogLevel.Warning => "WARN ",
                LogLevel.Error => "ERROR",
                LogLevel.Critical => "CRIT ",
                _ => "UNKN "
            };
        }
    }
}
