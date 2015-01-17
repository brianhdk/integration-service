using System;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments.MicroOrm
{
    public class MicroOrmTask : Task
    {
        private readonly IDbFactory _factory;

        public MicroOrmTask(IDbFactory factory)
        {
            _factory = factory;
        }

        public override string Description
        {
            get { return "TBD."; }
        }

        public override string Schedule
        {
            get { return "TBD."; }
        }

        public override void StartTask(Log log, params string[] arguments)
        {
            using (IDb db = _factory.OpenDatabase())
            {
                log.Message(db.SingleOrDefault<ErrorLog>(String.Format("SELECT TOP 1 * FROM {0}", Tables.ErrorLog)).Message);
            }
        }
    }
}