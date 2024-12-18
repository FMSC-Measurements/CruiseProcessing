using CruiseProcessing.Services.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace CruiseProcessing.Test
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Critical Code Smell", "S3881:Implement Dispose Pattern", Justification = "nothing to dispose")]
    public class NullScope : IDisposable
    {
        public static IDisposable Instance { get; } = new NullScope();

        public void Dispose()
        { /*do nothing*/ }
    }

    public class TestLogger : ILogger
    {
        public ITestOutputHelper Output { get; set; }

        public LogLevel MinLogLevel { get; set; }

        public string CategoryName { get; }

        public TestLogger(ITestOutputHelper output, LogLevel minLogLevel, string category)
        {
            Output = output ?? throw new ArgumentNullException(nameof(output));
            MinLogLevel = minLogLevel;
            CategoryName = category;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= MinLogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var message = formatter(state, exception);
            var properties = new Dictionary<string, string>
            {
                { nameof(message), message },
                { nameof(logLevel), GetLogLevelString(logLevel) }
            };

            if (state is IReadOnlyCollection<KeyValuePair<string, object>> stateParms)
            {
                foreach (var kvp in stateParms)
                {
                    if (!properties.ContainsKey(kvp.Key))
                    {
                        properties.Add(kvp.Key, kvp.Value?.ToString());
                    }
                }
            }

            if (exception != null) { properties.Add(nameof(exception), exception.ToString()); }

            if (logLevel == LogLevel.Critical || logLevel == LogLevel.Error)
            {
                Output.WriteLine($"[{logLevel.ToString().PadRight(11)}]{CategoryName}::Exception::{message}");
            }
            else
            {
                var eventName = eventId.Name
                    //?? GetValueOrDefault(properties, "{OriginalFormat}")
                    ?? ""; // if no event name is provided, use the unformatted original message

                Output.WriteLine($"[{logLevel.ToString().PadRight(11)}][{CategoryName}][{eventName}]{message}");
            }
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
                    return "Information";

                case LogLevel.Warning:
                    return "Warning";

                case LogLevel.Error:
                    return "Error";

                case LogLevel.Critical:
                    return "Critical";

                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        private static TValue GetValueOrDefault<TKey, TValue>(IDictionary<TKey, TValue> dict, TKey key)
        {
            if (dict.TryGetValue(key, out TValue value))
            { return value; }
            else { return default; }
        }
    }
}
