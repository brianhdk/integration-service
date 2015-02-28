using System;
using System.Collections.Generic;
using System.Data;
using Vertica.Integration.Infrastructure.Database.Dapper;
using Vertica.Integration.Properties;
using Vertica.Utilities_v4.Patterns;

namespace Vertica.Integration.Infrastructure.Logging
{
    public class Logger : ILogger
    {
        private readonly IDapperProvider _dapper;
        private readonly ISettings _settings;

        private readonly object _dummy = new object();
        private readonly Stack<object> _disablers;

        private readonly ChainOfResponsibilityLink<LogEntry> _chainOfResponsibility;

        public Logger(IDapperProvider dapper, ISettings settings)
        {
            _dapper = dapper;
            _settings = settings;

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

        public ErrorLog LogError(Exception exception, Target target = Target.Service)
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

            if (_disablers.Count > 0 || _settings.DisableDatabaseLog)
                return;

            _chainOfResponsibility.Handle(logEntry);
        }

        public IDisposable Disable()
        {
            _disablers.Push(_dummy);
            return new Disabler(() => _disablers.Pop());
        }

        private class Disabler : IDisposable
        {
            private readonly Action _disposed;

            public Disabler(Action disposed)
            {
                _disposed = disposed;
            }

            public void Dispose()
            {
                _disposed();
            }
        }

        private ErrorLog Log(ErrorLog errorLog)
        {
            if (_settings.DisableDatabaseLog)
                return null;

            using (IDapperSession session = _dapper.OpenSession())
            using (IDbTransaction transaction = session.BeginTransaction())
            {
                errorLog.Id = session.ExecuteScalar<int>(
                    @"INSERT INTO [ErrorLog] (MachineName, IdentityName, CommandLine, Severity, Message, FormattedMessage, TimeStamp, Target)
                      VALUES (@MachineName, @IdentityName, @CommandLine, @Severity, @Message, @FormattedMessage, @TimeStamp, @Target)
                      SELECT CAST(SCOPE_IDENTITY() AS INT)",
                    new
                    {
                        MachineName = errorLog.MachineName,
                        IdentityName = errorLog.IdentityName,
                        CommandLine = errorLog.CommandLine,
                        Severity = Enum.GetName(errorLog.Severity.GetType(), errorLog.Severity),
                        Message = errorLog.Message,
                        FormattedMessage = errorLog.FormattedMessage,
                        TimeStamp = errorLog.TimeStamp,
                        Target = Enum.GetName(errorLog.Target.GetType(), errorLog.Target)
                    });

                transaction.Commit();

                return errorLog;
            }
        }

        private abstract class LogEntryLink<TLogEntry> : IChainOfResponsibilityLink<LogEntry>
            where TLogEntry : LogEntry
        {
            private readonly IDapperProvider _dapper;

            protected LogEntryLink(IDapperProvider dapper)
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
                        HandleInsert(session, context as TLogEntry);
                    else
                        HandleUpdate(session, context as TLogEntry);

                    transaction.Commit();
                }
            }

            protected abstract void HandleInsert(IDapperSession session, TLogEntry logEntry);

            private void HandleUpdate(IDapperSession session, TLogEntry logEntry)
            {
                session.Execute(
                    @"UPDATE TaskLog SET ExecutionTimeSeconds = @ExecutionTimeSeconds WHERE Id = @Id",
                    new
                    {
                        ExecutionTimeSeconds = logEntry.ExecutionTimeSeconds,
                        Id = logEntry.Id
                    });
            }
        }

        private class MessageLogLink : LogEntryLink<MessageLog>
        {
            public MessageLogLink(IDapperProvider dapper)
                : base(dapper)
            {
            }

            protected override void HandleInsert(IDapperSession session, MessageLog logEntry)
            {
                logEntry.Id = session.ExecuteScalar<int>(
                    @"INSERT INTO TaskLog (TaskName, ExecutionTimeSeconds, TimeStamp, StepName, Message, TaskLog_Id, StepLog_Id, Type)
                      VALUES (@TaskName, @ExecutionTimeSeconds, @TimeStamp, @StepName, @Message, @TaskLog_Id, @StepLog_Id, 'M')
                      SELECT CAST(SCOPE_IDENTITY() AS INT)",
                    new
                    {
                        TaskName = logEntry.TaskName,
                        ExecutionTimeSeconds = logEntry.ExecutionTimeSeconds,
                        TimeStamp = logEntry.TimeStamp,
                        StepName = logEntry.StepName,
                        Message = logEntry.Message,
                        TaskLog_Id = logEntry.TaskLog.Id,
                        StepLog_Id = logEntry.StepLog != null ? logEntry.StepLog.Id : default(int?)
                    });
            }
        }

        private class StepLogLink : LogEntryLink<StepLog>
        {
            public StepLogLink(IDapperProvider dapper)
                : base(dapper)
            {
            }

            protected override void HandleInsert(IDapperSession session, StepLog logEntry)
            {
                logEntry.Id = session.ExecuteScalar<int>(
                    @"INSERT INTO TaskLog (TaskName, ExecutionTimeSeconds, TimeStamp, StepName, TaskLog_Id, ErrorLog_id, Type)
                      VALUES (@TaskName, @ExecutionTimeSeconds, @TimeStamp, @StepName, @TaskLog_Id, @ErrorLog_id, 'S')
                      SELECT CAST(SCOPE_IDENTITY() AS INT)",
                    new
                    {
                        TaskName = logEntry.TaskName,
                        ExecutionTimeSeconds = logEntry.ExecutionTimeSeconds,
                        TimeStamp = logEntry.TimeStamp,
                        StepName = logEntry.StepName,
                        TaskLog_Id = logEntry.TaskLog.Id,
                        ErrorLog_id = logEntry.ErrorLog != null ? logEntry.ErrorLog.Id : default(int?)
                    });
            }
        }

        private class TaskLogLink : LogEntryLink<TaskLog>
        {
            public TaskLogLink(IDapperProvider dapper)
                : base(dapper)
            {
            }

            protected override void HandleInsert(IDapperSession session, TaskLog logEntry)
            {
                logEntry.Id = session.ExecuteScalar<int>(
                    @"INSERT INTO TaskLog (TaskName, ExecutionTimeSeconds, TimeStamp, ErrorLog_id, Type)
                      VALUES (@TaskName, @ExecutionTimeSeconds, @TimeStamp, @ErrorLog_id, 'T')
                      SELECT CAST(SCOPE_IDENTITY() AS INT)",
                    new
                    {
                        TaskName = logEntry.TaskName,
                        ExecutionTimeSeconds = logEntry.ExecutionTimeSeconds,
                        TimeStamp = logEntry.TimeStamp,
                        ErrorLog_id = logEntry.ErrorLog != null ? logEntry.ErrorLog.Id : default(int?)
                    });
            }
        }
    }
}