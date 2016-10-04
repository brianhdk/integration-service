using System;
using System.Collections.Generic;
using Vertica.Utilities_v4.Patterns;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
    public abstract class Logger : ILogger
    {
        private readonly object _dummy = new object();
        private readonly Stack<object> _disablers;

        private readonly ChainOfResponsibilityLink<LogEntry> _handlers;

        protected Logger()
        {
            _disablers = new Stack<object>();

            _handlers = ChainOfResponsibility.Empty<LogEntry>()
                .Chain(new LogEntryLink<TaskLog>(Insert, Update))
                .Chain(new LogEntryLink<StepLog>(Insert, Update))
                .Chain(new LogEntryLink<MessageLog>(Insert));
        }

        public ErrorLog LogError(ITarget target, string message, params object[] args)
        {
            return LogError(new ErrorLog(Severity.Error, string.Format(message, args), target));
        }

        public ErrorLog LogError(Exception exception, ITarget target = null)
        {
            return LogError(new ErrorLog(exception, target));
        }

        public ErrorLog LogWarning(ITarget target, string message, params object[] args)
        {
            return LogError(new ErrorLog(Severity.Warning, string.Format(message, args), target));
        }

        private ErrorLog LogError(ErrorLog errorLog)
        {
            if (LoggingDisabled)
                return null;

            errorLog.Id = Insert(errorLog);

            return errorLog;
        }

        public void LogEntry(LogEntry entry)
        {
            if (entry == null) throw new ArgumentNullException(nameof(entry));

            if (LoggingDisabled)
                return;

            _handlers.Handle(entry);            
        }

        protected abstract string Insert(TaskLog log);
        protected abstract string Insert(MessageLog log);
        protected abstract string Insert(StepLog log);

        protected abstract string Insert(ErrorLog log);

        protected abstract void Update(TaskLog log);
        protected abstract void Update(StepLog log);

        public virtual IDisposable Disable()
        {
            _disablers.Push(_dummy);
            return new Disabler(() => _disablers.Pop());
        }

        protected virtual bool LoggingDisabled => _disablers.Count > 0;

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

        private class LogEntryLink<TLogEntry> : IChainOfResponsibilityLink<LogEntry>
            where TLogEntry : LogEntry
        {
            private readonly Func<TLogEntry, string> _insert;
            private readonly Action<TLogEntry> _update;

            public LogEntryLink(Func<TLogEntry, string> insert, Action<TLogEntry> update = null)
            {
                if (insert == null) throw new ArgumentNullException(nameof(insert));

                _insert = insert;
                _update = update;
            }

            public bool CanHandle(LogEntry context)
            {
                return context is TLogEntry;
            }

            public void DoHandle(LogEntry context)
            {
                if (context.Id == null)
                {
                    context.Id = _insert(context as TLogEntry);
                }
                else
                {
                    if (_update == null)
                        throw new NotSupportedException(
	                        $"Update for '{typeof (TLogEntry).Name}' is not supported.");                        
                    
                    _update(context as TLogEntry);
                }
            }
        }
    }
}