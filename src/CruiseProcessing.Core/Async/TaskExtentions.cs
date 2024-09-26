using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

#nullable enable

namespace CruiseProcessing.Async
{
    public static class TaskExtentions
    {
        public static ILogger? Logger { get; set; }

        public static Action<Exception>? OnException { get; set; }

        public static bool RethrowExceptions { get; set; }

        public static void FireAndForget(this Task @this, string? failMessage = null)
        {
            @this.ContinueWith(HandelTaskException, failMessage, TaskContinuationOptions.OnlyOnFaulted);
        }

        private static void HandelTaskException(Task t, object state)
        {
            var exception = t.Exception;
            if (exception is null) { return; }
            var failMessage = state as string;

            OnException?.Invoke(exception);
            foreach (var ex in exception.InnerExceptions)
            {
                Logger?.LogError(ex, "Async Task Exception: {failMessage}", failMessage);
            }

            if (RethrowExceptions)
            { throw exception; }
        }
    }
}