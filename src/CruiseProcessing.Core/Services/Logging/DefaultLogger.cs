using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace CruiseProcessing.Services.Logging
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3881:Implement Dispose Pattern", Justification = "nothing to dispose")]
    public class NullScope : IDisposable
    {
        public static IDisposable Instance { get; } = new NullScope();

        public void Dispose()
        { /*do nothing*/ }
    }

    public class DefaultLogger : ILogger
    {
        public LogLevel MinLogLevel { get; set; }

        public string CategoryName { get; }

        //public DefaultLogger()
        //    : this(LogLevel.Information)
        //{ }

        public DefaultLogger(LogLevel minLogLevel, string? category = null)
        {
            CategoryName = category ?? typeof(DefaultLogger).Name;
            MinLogLevel = minLogLevel;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= MinLogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);

            var logLevelString = GetLogLevelString(logLevel).PadRight(5);

            if (exception != null)
            {
                message = "Exception-" + message;
            }

            var eventName = eventId.Name
                ?? "";

            Debug.WriteLine($"[{logLevelString}]{eventName}-{message}", CategoryName);
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return "Trace";

                case LogLevel.Debug:
                    return "Debug";

                case LogLevel.Information:
                    return "Info";

                case LogLevel.Warning:
                    return "Warn";

                case LogLevel.Error:
                    return "Error";

                case LogLevel.Critical:
                    return "Crit";

                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }
    }
}
