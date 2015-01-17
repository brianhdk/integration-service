using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NHibernate;
using PetaPoco;
using Vertica.Integration.Infrastructure.Database.NHibernate;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4.Extensions.StringExt;

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
ORDER BY timestamp DESC
", count);

            IEnumerable<TaskLog> tasks;

            using (IStatelessSession session = _sessionFactory.SessionFactory.OpenStatelessSession())
            using (Database db = new PetaPoco.Database(session.Connection))
            {
                tasks = db.Query<TaskLog>(query).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, tasks);
        }

		public HttpResponseMessage Get()
		{
			var query = string.Format(@"
SELECT [TaskName]
  FROM [TaskLog] group by TaskName");

			IEnumerable<string> taskNames;

			using (IStatelessSession session = _sessionFactory.SessionFactory.OpenStatelessSession())
			using (Database db = new PetaPoco.Database(session.Connection))
			{
				taskNames = db.Query<string>(query).ToList();
			}

			return Request.CreateResponse(HttpStatusCode.OK, taskNames);
		}
    }
}
