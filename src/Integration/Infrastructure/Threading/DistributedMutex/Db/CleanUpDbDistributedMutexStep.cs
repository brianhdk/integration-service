using System;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Model;
using Vertica.Utilities;

namespace Vertica.Integration.Infrastructure.Threading.DistributedMutex.Db
{
    public class CleanUpDbDistributedMutexStep : Step<MaintenanceWorkItem>
    {
        private readonly Lazy<IDeleteDbDistributedMutexLocksCommand> _deleteCommand;
        private readonly IIntegrationDatabaseConfiguration _configuration;

        public CleanUpDbDistributedMutexStep(Lazy<IDeleteDbDistributedMutexLocksCommand> deleteCommand, IIntegrationDatabaseConfiguration configuration)
        {
            _deleteCommand = deleteCommand;
            _configuration = configuration;
        }

        public override Execution ContinueWith(ITaskExecutionContext<MaintenanceWorkItem> context)
        {
            if (_configuration.Disabled)
                return Execution.StepOver;

            return Execution.Execute;
        }

        public override void Execute(ITaskExecutionContext<MaintenanceWorkItem> context)
        {
            DateTimeOffset olderThanOneDay = Time.UtcNow.AddHours(-24);

            int deletions = _deleteCommand.Value.Execute(olderThanOneDay);

            if (deletions > 0)
                context.Log.Message("Deleted {0} records locked before {1}", deletions, olderThanOneDay);

        }

        public override string Description => "Deletes lock records older than 24 hours.";
    }
}