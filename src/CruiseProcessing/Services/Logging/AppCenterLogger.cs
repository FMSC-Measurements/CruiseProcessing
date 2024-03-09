using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Services.Logging
{
    public class AppCenterLogger : ILogger
    {
        public string CategoryName { get; }

        public AppCenterLogger(string name) 
        {
            CategoryName = name;
        }

        internal IExternalScopeProvider ScopeProvider { get; set; }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            // If the filter is null, everything is enabled
            // unless the debugger is not attached
            return logLevel != LogLevel.None;
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

            if(exception != null) { properties.Add(nameof(exception), exception.ToString()); }

            if(logLevel == LogLevel.Critical || logLevel == LogLevel.Error)
            {
                Crashes.TrackError(exception, properties);
            }
            else
            {
                Analytics.TrackEvent(CategoryName + ":" + eventId.Name, properties);
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


    }
}
