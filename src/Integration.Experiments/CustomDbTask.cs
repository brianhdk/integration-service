using System.Data;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class CustomDbTask : Task
    {
        private readonly IDbFactory _integrationDb;
        private readonly IDbFactory<CustomDb> _customDb;

        public CustomDbTask(IDbFactory integrationDb, IDbFactory<CustomDb> customDb)
        {
            _integrationDb = integrationDb;
            _customDb = customDb;
        }

        public override string Description
        {
            get { return "TBD"; }
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            using (IDbSession session = _integrationDb.OpenSession())
            using (IDbTransaction transaction = session.Connection.BeginTransaction())
            {
                transaction.Commit();
            }
        }
    }
}