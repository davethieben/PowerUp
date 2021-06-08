using System;
using Microsoft.Extensions.Logging;

namespace PowerUp.Helpers
{
    public class CallbackLoggerProvider : ILoggerProvider
    {
        private readonly Action<string> _callback;

        public CallbackLoggerProvider(Action<string> callback)
        {
            _callback = callback;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new CallbackLogger(categoryName, _callback);
        }

        public void Dispose()
        {
        }

        private class CallbackLogger : ILogger
        {
            private readonly string _loggerName;
            private readonly Action<string> _callback;

            public CallbackLogger(string loggerName, Action<string> callback)
            {
                _loggerName = loggerName;
                _callback = callback;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                throw new NotImplementedException();
            }

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, 
                Func<TState, Exception, string> formatter)
            {
                _callback.Invoke(
                    $"[{_loggerName}] {formatter.Invoke(state, exception)}{Environment.NewLine}");
            }

        }
    }
}
