using Smartstore.Utilities;

namespace Microsoft.Extensions.Logging
{
    public static partial class LoggerExtensions
    {
        #region Is[X]Enabled

        public static bool IsTraceEnabled(this ILogger l)
        {
            return l.IsEnabled(LogLevel.Trace);
        }

        public static bool IsDebugEnabled(this ILogger l)
        {
            return l.IsEnabled(LogLevel.Debug);
        }

        public static bool IsInfoEnabled(this ILogger l)
        {
            return l.IsEnabled(LogLevel.Information);
        }

        public static bool IsWarnEnabled(this ILogger l)
        {
            return l.IsEnabled(LogLevel.Warning);
        }

        public static bool IsErrorEnabled(this ILogger l)
        {
            return l.IsEnabled(LogLevel.Error);
        }

        public static bool IsCriticalEnabled(this ILogger l)
        {
            return l.IsEnabled(LogLevel.Critical);
        }

        #endregion

        #region Log methods

        public static void Trace(this ILogger l, string msg, params object[] args)
        {
            l.LogTrace(msg, args);
            l.LogMessageToSlackLogger(LogLevel.Trace, null, msg, args);
        }

        public static void Trace(this ILogger l, Exception ex, params object[] args)
        {
            l.LogTrace(ex, null, args);
            l.LogMessageToSlackLogger(LogLevel.Trace,ex, null, args);
        }

        public static void Trace(this ILogger l, Func<string> msgFactory, params object[] args)
        {
            if (l.IsEnabled(LogLevel.Trace))
            {
                l.LogTrace(msgFactory(), args);
                l.LogMessageToSlackLogger(LogLevel.Trace, null, msgFactory(), args);
            }
        }

        public static void Trace(this ILogger l, Exception ex, string msg, params object[] args)
        {
            l.LogTrace(ex, msg, args);
            l.LogMessageToSlackLogger(LogLevel.Trace, ex, msg, args);
        }


        public static void Debug(this ILogger l, string msg, params object[] args)
        {
            l.LogDebug(msg, args);
            l.LogMessageToSlackLogger(LogLevel.Debug, null, msg, args);
        }

        public static void Debug(this ILogger l, Exception ex, params object[] args)
        {
            l.LogDebug(ex, null, args);
            l.LogMessageToSlackLogger(LogLevel.Debug, ex, null, args);
        }

        public static void Debug(this ILogger l, Func<string> msgFactory, params object[] args)
        {
            if (l.IsEnabled(LogLevel.Debug))
            {
                l.LogMessageToSlackLogger(LogLevel.Debug, null, msgFactory(), args);
                l.LogDebug(msgFactory(), args);
            }
        }

        public static void Debug(this ILogger l, Exception ex, string msg, params object[] args)
        {
            l.LogMessageToSlackLogger(LogLevel.Debug, ex, msg, args);
            l.LogDebug(ex, msg, args);
        }


        public static void Info(this ILogger l, string msg, params object[] args)
        {
            l.LogMessageToSlackLogger(LogLevel.Information, null, msg, args);
            l.LogInformation(msg, args);
        }

        public static void Info(this ILogger l, Exception ex, params object[] args)
        {
            l.LogMessageToSlackLogger(LogLevel.Information, ex, null, args);
            l.LogInformation(ex, null, args);
        }

        public static void Info(this ILogger l, Func<string> msgFactory, params object[] args)
        {
            if (l.IsEnabled(LogLevel.Information))
            {
                l.LogMessageToSlackLogger(LogLevel.Information, null, msgFactory(), args);
                l.LogInformation(msgFactory(), args);
            }
        }

        public static void Info(this ILogger l, Exception ex, string msg, params object[] args)
        {
            l.LogMessageToSlackLogger(LogLevel.Information, ex, msg, args);
            l.LogInformation(ex, msg, args);
        }


        public static void Warn(this ILogger l, string msg, params object[] args)
        {
            l.LogMessageToSlackLogger(LogLevel.Warning, null, msg, args);
            l.LogWarning(msg, args);
        }

        public static void Warn(this ILogger l, Exception ex, params object[] args)
        {
            l.LogMessageToSlackLogger(LogLevel.Warning, ex, null, args);
            l.LogWarning(ex, null, args);
        }

        public static void Warn(this ILogger l, Func<string> msgFactory, params object[] args)
        {
            if (l.IsEnabled(LogLevel.Warning))
            {
                l.LogMessageToSlackLogger(LogLevel.Warning, null, msgFactory(), args);
                l.LogWarning(msgFactory(), args);
            }
        }

        public static void Warn(this ILogger l, Exception ex, string msg, params object[] args)
        {
            l.LogMessageToSlackLogger(LogLevel.Warning, ex, msg, args);
            l.LogWarning(ex, msg, args);
        }


        public static void Error(this ILogger l, string msg, params object[] args)
        {
            l.LogMessageToSlackLogger(LogLevel.Error, null, msg, args);
            l.LogError(msg, args);
        }

        public static void Error(this ILogger l, Exception ex, params object[] args)
        {
            l.LogMessageToSlackLogger(LogLevel.Error, ex, null, args);
            l.LogError(ex, ex.Message, args);
        }

        public static void Error(this ILogger l, Func<string> msgFactory, params object[] args)
        {
            if (l.IsEnabled(LogLevel.Error))
            {
                l.LogMessageToSlackLogger(LogLevel.Error, null, msgFactory(), args);
                l.LogError(msgFactory(), args);
            }
        }

        public static void Error(this ILogger l, Exception ex, string msg, params object[] args)
        {
            l.LogMessageToSlackLogger(LogLevel.Error, ex, msg, args);
            l.LogError(ex, msg, args);
        }

        public static void ErrorsAll(this ILogger l, Exception exception)
        {
            if (!l.IsEnabled(LogLevel.Error))
            {
                return;
            }

            while (exception != null)
            {
                l.LogMessageToSlackLogger(LogLevel.Error, exception, exception.Message);
                l.LogError(exception, exception.Message);
                exception = exception.InnerException;
            }
        }

        public static void Critical(this ILogger l, string msg, params object[] args)
        {
            l.LogMessageToSlackLogger(LogLevel.Critical, null, msg, args);
            l.LogCritical(msg, args);
        }

        public static void Critical(this ILogger l, Exception ex, params object[] args)
        {
            l.LogMessageToSlackLogger(LogLevel.Critical, ex, null, args);
            l.LogCritical(ex, null, args);
        }

        public static void Critical(this ILogger l, Func<string> msgFactory, params object[] args)
        {
            if (l.IsEnabled(LogLevel.Critical))
            {
                l.LogMessageToSlackLogger(LogLevel.Critical, ex: null, msgFactory(), args);
                l.LogCritical(msgFactory(), args);
            }
        }

        public static void Critical(this ILogger l, Exception ex, string msg, params object[] args)
        {
            l.LogMessageToSlackLogger(LogLevel.Critical, ex, msg, args);
            l.LogCritical(ex, msg, args);
        }

        #endregion
    }
}
