using System.Data;
using Vertica.Integration.Infrastructure.Database.Dapper;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments.Custom_Database
{
    public class CustomDbTesterTask : Task
    {
        private readonly IDapperFactory<CustomDb> _customDb;

        public CustomDbTesterTask(IDapperFactory<CustomDb> customDb)
        {
            _customDb = customDb;
        }

        public override string Description
        {
            get { return "TBD"; }
        }

        public override void StartTask(Log log, params string[] arguments)
        {
            using (IDapperSession session = _customDb.OpenSession())
            using (IDbTransaction transaction = session.BeginTransaction())
            {
                session.Execute("INSERT INTO Test (Name) Values ('some name');");
                log.Message("Number of rows: {0}", session.ExecuteScalar<int>("SELECT COUNT(Id) FROM Test"));

                transaction.Commit();
            }
        }
    }
}