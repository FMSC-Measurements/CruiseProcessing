using Microsoft.Extensions.Logging;

namespace CruiseProcessing.Services.Logging
{
    public static class LoggingBuilderExtentions
    {
        public static void AddAppCenterLogger(this ILoggingBuilder builder)
        {
            builder.AddProvider(new AppCenterLoggerProvider());
        }
    }
}