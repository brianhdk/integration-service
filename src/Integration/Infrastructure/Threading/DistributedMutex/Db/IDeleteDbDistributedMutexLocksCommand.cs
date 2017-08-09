using System;
using System.Data;
using Vertica.Integration.Infrastructure.Database;

namespace Vertica.Integration.Infrastructure.Threading.DistributedMutex.Db
{
    public interface IDeleteDbDistributedMutexLocksCommand
    {
        int Execute(DateTimeOffset olderThan);
    }

    public class DeleteDbDistributedMutexLocksCommand : IDeleteDbDistributedMutexLocksCommand
    {
        private readonly IDbFactory _db;
        private readonly IIntegrationDatabaseConfiguration _configuration;

        public DeleteDbDistributedMutexLocksCommand(IDbFactory db, IIntegrationDatabaseConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public int Execute(DateTimeOffset olderThan)
        {
            using (IDbSession session = _db.OpenSession())
            using (IDbTransaction transaction = session.BeginTransaction())
            {
                int deletions = session.Execute($@"
DELETE FROM [{_configuration.TableName(IntegrationDbTable.DistributedMutex)}] WHERE (CreatedAt <= @olderThan)", new { olderThan });

                transaction.Commit();

                return deletions;
            }
        }
    }
}