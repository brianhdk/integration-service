using System;
using System.Data;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Model;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Threading.DistributedMutex.Db
{
    public class CleanUpDbDistributedMutexStep : Step<MaintenanceWorkItem>
    {
        private readonly Lazy<IDbFactory> _db;
        private readonly IIntegrationDatabaseConfiguration _configuration;

        public CleanUpDbDistributedMutexStep(Lazy<IDbFactory> db, IIntegrationDatabaseConfiguration confiugration)
        {
            _db = db;
            _configuration = confiugration;
        }

        public override Execution ContinueWith(MaintenanceWorkItem workItem)
        {
            if (_configuration.Disabled)
                return Execution.StepOver;

            return Execution.Execute;
        }

        public override void Execute(MaintenanceWorkItem workItem, ITaskExecutionContext context)
        {
            using (IDbSession session = _db.Value.OpenSession())
            using (IDbTransaction transaction = session.BeginTransaction())
            {
                DateTimeOffset deleteBefore = Time.UtcNow.AddHours(-24);

                var deletions = session.Execute(@"DELETE FROM DistributedMutex WHERE (CreatedAt <= @deleteBefore)", new { deleteBefore });

                if (deletions > 0)
                    context.Log.Message("Deleted {0} records locked before {1}", deletions, deleteBefore);

                transaction.Commit();
            }
        }

        public override string Description => "Deletes lock records older than 24 hours.";
    }
}