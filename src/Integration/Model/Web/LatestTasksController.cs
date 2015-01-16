using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NHibernate;
using PetaPoco;
using Vertica.Integration.Infrastructure.Database.NHibernate;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model.Web
{
    public class LatestTasksController : ApiController
    {
        private readonly ISessionFactoryProvider _sessionFactory;

        public LatestTasksController(ISessionFactoryProvider sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public HttpResponseMessage Get(int count)
        {
            var query = string.Format(@"
SELECT TOP {0}
	[Id],
	[Type],
	[TaskName],
	[StepName],
	[Message],
	[ExecutionTimeSeconds],
	[TimeStamp],
	[TaskLog_Id],
	[StepLog_Id],
	[ErrorLog_Id]
FROM [TaskLog]
WHERE stepname IS NOT null
ORDER BY timestamp DESC
", count == 0 ? 10 : count);

            IEnumerable<TaskLog> tasks;

            using (IStatelessSession session = _sessionFactory.SessionFactory.OpenStatelessSession())
            using (Database db = new PetaPoco.Database(session.Connection))
            {
                tasks = db.Query<TaskLog>(query).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, tasks);
        }
    }
}
