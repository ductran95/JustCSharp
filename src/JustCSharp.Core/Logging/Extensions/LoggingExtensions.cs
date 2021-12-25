using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace JustCSharp.Core.Logging.Extensions
{
    public static class LoggingExtensions
    {
        public static Stopwatch StartStopwatch(this ILogger logger)
        {
            Stopwatch stopwatch = null;
            if (logger.IsEnabled(LogLevel.Trace))
            {
                stopwatch = Stopwatch.StartNew();
            }

            return stopwatch;
        }
        
        public static string GetElapsedTime(this ILogger logger, Stopwatch stopwatch)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                return stopwatch?.ToString();
            }

            return "Elapsed time only available when Trace is enabled";
        }
    }
}

