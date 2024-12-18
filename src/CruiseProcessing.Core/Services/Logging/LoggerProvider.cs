using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CruiseProcessing.Services.Logging
{
    public static class LoggerProvider
    {
        public static ILoggerFactory DefaultLoggerFactory { get; set; } = new DefaultLoggerFactory();

        public static IServiceProvider? Services { get; set; }

        public static void Initialize(IServiceProvider services)
        {
            Services = services;
        }

        public static ILogger<T> CreateLogger<T>()
        {
            var loggerFactory = Services?.GetService<ILoggerFactory>() ?? DefaultLoggerFactory;
            return loggerFactory!.CreateLogger<T>();
        }

        public static ILogger CreateLogger(Type type)
        {
            var loggerFactory = Services?.GetService<ILoggerFactory>() ?? DefaultLoggerFactory;
            return loggerFactory!.CreateLogger(type);
        }

        public static ILogger CreateLogger(string categoryName)
        {
            var loggerFactory = Services?.GetService<ILoggerFactory>() ?? DefaultLoggerFactory;
            return loggerFactory!.CreateLogger(categoryName);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S3881:Implement Dispose Pattern", Justification = "nothing to dispose")]
    public class DefaultLoggerFactory : ILoggerFactory
    {
        public LogLevel MinLogLevel { get; set; } = LogLevel.Information;

        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotSupportedException();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DefaultLogger(MinLogLevel, categoryName);
        }

        public void Dispose()
        {
            /* do nothing */
        }
    }
}
