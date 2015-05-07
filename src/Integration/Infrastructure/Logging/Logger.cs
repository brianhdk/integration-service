using System;
using System.Collections.Generic;
using System.Data;
using Vertica.Integration.Infrastructure.Database.Dapper;
using Vertica.Integration.Infrastructure.Database.Dapper.Extensions;
using Vertica.Utilities_v4.Patterns;

namespace Vertica.Integration.Infrastructure.Logging
{
    public class Logger : ILogger
    {
        private readonly IDapperFactory _dapper;

        private readonly object _dummy = new object();
        private readonly Stack<object> _disablers;

        private readonly ChainOfResponsibilityLink<LogEntry> _chainOfResponsibility;

        public Logger(IDapperFactory dapper)
        {
            _dapper = dapper;

            _chainOfResponsibility = ChainOfResponsibility.Empty<LogEntry>()
                .Chain(new TaskLogLink(_dapper))
                .Chain(new StepLogLink(_dapper))
                .Chain(new MessageLogLink(_dapper));

            _disablers = new Stack<object>();
        }

        public ErrorLog LogError(Target target, string message, params object[] args)
        {
            return Log(new ErrorLog(Severity.Error, String.Format(message, args), target));
        }

        public ErrorLog LogError(Exception exception, Target target = null)
        {
            if (exception == null) throw new ArgumentNullException("exception");

            return Log(new ErrorLog(exception, target));
        }

        public ErrorLog LogWarning(Target target, string message, params object[] args)
        {
            return Log(new ErrorLog(Severity.Warning, String.Format(message, args), target));
        }

        public void LogEntry(LogEntry logEntry)
        {
            if (logEntry == null) throw new ArgumentNullException("logEntry");

            if (LoggingDisabled)
                return;

            _chainOfResponsibility.Handle(logEntry);
        }

        public IDisposable Disable()
        {
            _disablers.Push(_dummy);
            return new Disabler(() => _disablers.Pop());
        }

        private bool LoggingDisabled
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

        private ErrorLog Log(ErrorLog errorLog)
        {
            if (LoggingDisabled)
                return null;

            using (IDapperSession session = _dapper.OpenSession())
            using (IDbTransaction transaction = session.BeginTransaction())
            {
                errorLog.Id = session.Wrap(s => s.ExecuteScalar<int>(
                    @"INSERT INTO [ErrorLog] (MachineName, IdentityName, CommandLine, Severity, Message, FormattedMessage, TimeStamp, Target)
                      VALUES (@MachineName, @IdentityName, @CommandLine, @Severity, @Message, @FormattedMessage, @TimeStamp, @Target)
                      SELECT CAST(SCOPE_IDENTITY() AS INT)",
                    new
                    {
                        errorLog.MachineName, 
                        errorLog.IdentityName, 
                        errorLog.CommandLine,
                        Severity = errorLog.Severity.ToString(), 
                        errorLog.Message, 
                        errorLog.FormattedMessage, 
                        errorLog.TimeStamp,
                        Target = errorLog.Target.ToString()
                    }));

                transaction.Commit();

                return errorLog;
            }
        }

        private abstract class LogEntryLink<TLogEntry> : IChainOfResponsibilityLink<LogEntry>
            where TLogEntry : LogEntry
        {
            private readonly IDapperFactory _dapper;

            protected LogEntryLink(IDapperFactory dapper)
            {
                _dapper = dapper;
            }

            public bool CanHandle(LogEntry context)
            {
                return context is TLogEntry;
            }

            public void DoHandle(LogEntry context)
            {
                using (IDapperSession session = _dapper.OpenSession())
                using (IDbTransaction transaction = session.BeginTransaction())
                {
                    if (context.Id == 0)
                    {
                        HandleInsert(session, context as TLogEntry);
                    }
                    else
                    {
                        HandleUpdate(session, context as TLogEntry);
                    }

                    transaction.Commit();
                }
            }

            protected abstract void HandleInsert(IDapperSession session, TLogEntry logEntry);
            protected abstract void HandleUpdate(IDapperSession session, TLogEntry logEntry);
        }

        private class MessageLogLink : LogEntryLink<MessageLog>
        {
            public MessageLogLink(IDapperFactory dapper)
                : base(dapper)
            {
            }

            protected override void HandleInsert(IDapperSession session, MessageLog logEntry)
            {
                logEntry.Id = session.Wrap(s => s.ExecuteScalar<int>(
                    @"INSERT INTO TaskLog (Type, TaskName, ExecutionTimeSeconds, TimeStamp, StepName, Message, TaskLog_Id, StepLog_Id)
                      VALUES ('M', @TaskName, @ExecutionTimeSeconds, @TimeStamp, @StepName, @Message, @TaskLog_Id, @StepLog_Id)
                      SELECT CAST(SCOPE_IDENTITY() AS INT)",
                    new
                    {
                        logEntry.TaskName,
                        logEntry.ExecutionTimeSeconds,
                        logEntry.TimeStamp,
                        logEntry.StepName,
                        logEntry.Message,
                        TaskLog_Id = logEntry.TaskLog.Id,
                        StepLog_Id = logEntry.StepLog != null ? logEntry.StepLog.Id : default(int?)
                    }));
            }

            protected override void HandleUpdate(IDapperSession session, MessageLog logEntry)
            {
                throw new NotSupportedException();
            }
        }

        private class StepLogLink : LogEntryLink<StepLog>
        {
            public StepLogLink(IDapperFactory dapper)
                : base(dapper)
            {
            }

            protected override void HandleInsert(IDapperSession session, StepLog logEntry)
            {
                logEntry.Id = session.Wrap(s => s.ExecuteScalar<int>(
                    @"INSERT INTO TaskLog (Type, TaskName, StepName, ExecutionTimeSeconds, TimeStamp, TaskLog_Id, ErrorLog_Id)
                      VALUES ('S', @TaskName, @StepName, @ExecutionTimeSeconds, @TimeStamp, @TaskLog_Id, @ErrorLog_Id)
                      SELECT CAST(SCOPE_IDENTITY() AS INT)",
                    new
                    {
                        logEntry.TaskName,
                        logEntry.StepName,
                        logEntry.ExecutionTimeSeconds,
                        logEntry.TimeStamp,
                        TaskLog_Id = logEntry.TaskLog.Id,
                        ErrorLog_Id = logEntry.ErrorLog != null ? logEntry.ErrorLog.Id : default(int?),

                    }));
            }

            protected override void HandleUpdate(IDapperSession session, StepLog logEntry)
            {
                session.Execute(
                    @"UPDATE TaskLog SET ExecutionTimeSeconds = @ExecutionTimeSeconds, ErrorLog_Id = @ErrorLog_Id WHERE Id = @Id",
                    new
                    {
                        logEntry.Id,
                        logEntry.ExecutionTimeSeconds,
                        ErrorLog_Id = logEntry.ErrorLog != null ? logEntry.ErrorLog.Id : default(int?)
                    });
            }
        }

        private class TaskLogLink : LogEntryLink<TaskLog>
        {
            public TaskLogLink(IDapperFactory dapper)
                : base(dapper)
            {
            }

            protected override void HandleInsert(IDapperSession session, TaskLog logEntry)
            {
                logEntry.Id = session.Wrap(s => s.ExecuteScalar<int>(
                    @"INSERT INTO TaskLog (Type, TaskName, ExecutionTimeSeconds, TimeStamp, MachineName, IdentityName, CommandLine, ErrorLog_Id)
                      VALUES ('T', @TaskName, @ExecutionTimeSeconds, @TimeStamp, @MachineName, @IdentityName, @CommandLine, @ErrorLog_Id)
                      SELECT CAST(SCOPE_IDENTITY() AS INT)",
                    new
                    {
                        logEntry.TaskName,
                        logEntry.ExecutionTimeSeconds,
                        logEntry.TimeStamp,
                        logEntry.MachineName,
                        logEntry.IdentityName,
                        logEntry.CommandLine,
                        ErrorLog_Id = logEntry.ErrorLog != null ? logEntry.ErrorLog.Id : default(int?)
                    }));
            }

            protected override void HandleUpdate(IDapperSession session, TaskLog logEntry)
            {
                session.Execute(
                    @"UPDATE TaskLog SET ExecutionTimeSeconds = @ExecutionTimeSeconds, ErrorLog_Id = @ErrorLog_Id WHERE Id = @Id",
                    new
                    {
                        logEntry.Id,
                        logEntry.ExecutionTimeSeconds,
                        ErrorLog_Id = logEntry.ErrorLog != null ? logEntry.ErrorLog.Id : default(int?)
                    });
            }
        }
    }
}