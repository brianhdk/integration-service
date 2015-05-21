using System;
using System.Collections.Generic;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
    public abstract class Logger : ILogger
    {
        private readonly object _dummy = new object();
        private readonly Stack<object> _disablers;

        protected Logger()
        {
            _disablers = new Stack<object>();            
        }

        public abstract ErrorLog LogWarning(ITarget target, string message, params object[] args);
        public abstract ErrorLog LogError(ITarget target, string message, params object[] args);
        public abstract ErrorLog LogError(Exception exception, ITarget target = null);
        public abstract void LogEntry(LogEntry entry);

        public IDisposable Disable()
        {
            _disablers.Push(_dummy);
            return new Disabler(() => _disablers.Pop());
        }

        protected bool LoggingDisabled
        {
            get { return _disablers.Count > 0; }
        }

        private class Disabler : IDisposable
        {
            private readonly Action _disposed;
            private bool _wasDisposed;

            public Disabler(Action disposed)
            {
                _disposed = disposed;
            }

            public void Dispose()
            {
                if (!_wasDisposed)
                {
                    _disposed();
                    _wasDisposed = true;
                }
            }
        }
    }
}