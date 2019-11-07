using System;
using System.Data;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Database.Extensions;
using Vertica.Integration.Infrastructure.Features;

namespace Vertica.Integration.Infrastructure.Logging.Loggers
{
    internal class DbLogger : Logger
    {
        private readonly IDbFactory _db;
        private readonly IIntegrationDatabaseConfiguration _configuration;
        private readonly IFeatureToggler _featureToggler;

        public DbLogger(IDbFactory db, IIntegrationDatabaseConfiguration configuration, IFeatureToggler featureToggler)
        {
            _db = db;
            _configuration = configuration;
            _featureToggler = featureToggler;
        }

        protected override bool LoggingDisabled => base.LoggingDisabled || _featureToggler.IsDisabled<DbLogger>();

        protected override string Insert(TaskLog log)
        {
            return Persist(session => session.ExecuteScalar<int>($@"
INSERT INTO [{_configuration.TableName(IntegrationDbTable.TaskLog)}] (Type, TaskName, TimeStamp, MachineName, IdentityName, CommandLine)
VALUES ('T', @TaskName, @TimeStamp, @MachineName, @IdentityName, @CommandLine)
SELECT CAST(SCOPE_IDENTITY() AS INT)",
                new
                {
                    TaskName = log.Name,
                    log.TimeStamp,
                    log.MachineName,
                    log.IdentityName,
                    log.CommandLine
                })).ToString();
        }

        protected override string Insert(MessageLog log)
        {
            return Persist(session => session.ExecuteScalar<int>($@"
INSERT INTO [{_configuration.TableName(IntegrationDbTable.TaskLog)}] (Type, TaskName, TimeStamp, StepName, Message, TaskLog_Id, StepLog_Id)
VALUES ('M', @TaskName, @TimeStamp, @StepName, @Message, @TaskLog_Id, @StepLog_Id)
SELECT CAST(SCOPE_IDENTITY() AS INT)",
                new
                {
                    TaskName = log.TaskLog.Name,
                    log.TimeStamp,
                    StepName = log.StepLog?.Name,
                    log.Message,
                    TaskLog_Id = log.TaskLog.Id,
                    StepLog_Id = log.StepLog?.Id
                })).ToString();
        }

        protected override string Insert(StepLog log)
        {
            return Persist(session => session.ExecuteScalar<int>($@"
INSERT INTO [{_configuration.TableName(IntegrationDbTable.TaskLog)}] (Type, TaskName, StepName, TimeStamp, TaskLog_Id)
VALUES ('S', @TaskName, @StepName, @TimeStamp, @TaskLog_Id)
SELECT CAST(SCOPE_IDENTITY() AS INT)",
                new
                {
                    TaskName = log.TaskLog.Name,
                    StepName = log.Name,
                    log.TimeStamp,
                    TaskLog_Id = log.TaskLog.Id
                })).ToString();
        }

        protected override string Insert(ErrorLog log)
        {
            return Persist(session => session.ExecuteScalar<int>($@"
INSERT INTO [{_configuration.TableName(IntegrationDbTable.ErrorLog)}] (MachineName, IdentityName, CommandLine, Severity, Message, FormattedMessage, TimeStamp, Target)
VALUES (@MachineName, @IdentityName, @CommandLine, @Severity, @Message, @FormattedMessage, @TimeStamp, @Target)
SELECT CAST(SCOPE_IDENTITY() AS INT)",
                new
                {
                    log.MachineName,
                    log.IdentityName,
                    log.CommandLine,
                    Severity = log.Severity.ToString(),
                    log.Message,
                    log.FormattedMessage,
                    log.TimeStamp,
                    Target = log.Target.ToString()
                })).ToString();
        }

        protected override void Update(TaskLog log)
        {
            Persist(session => session.Execute($@"
UPDATE [{_configuration.TableName(IntegrationDbTable.TaskLog)}] SET ExecutionTimeSeconds = @ExecutionTimeSeconds, ErrorLog_Id = @ErrorLog_Id WHERE Id = @Id",
                new
                {
                    log.Id,
                    ExecutionTimeSeconds = log.ExecutionTimeSeconds.GetValueOrDefault(),
                    ErrorLog_Id = log.ErrorLog?.Id
                }));
        }

        protected override void Update(StepLog log)
        {
            Persist(session => session.Execute($@"
UPDATE [{_configuration.TableName(IntegrationDbTable.TaskLog)}] SET ExecutionTimeSeconds = @ExecutionTimeSeconds, ErrorLog_Id = @ErrorLog_Id WHERE Id = @Id",
                new
                {
                    log.Id,
                    ExecutionTimeSeconds = log.ExecutionTimeSeconds.GetValueOrDefault(),
                    ErrorLog_Id = log.ErrorLog?.Id
                }));
        }

        private int Persist(Func<IDbSession, int> persist)
        {
            int result;

            using (IDbSession session = _db.OpenSession())
            using (IDbTransaction transaction = session.BeginTransaction())
            {
                result = session.Wrap(persist);

                transaction.Commit();
            }

            return result;
        }
    }
}