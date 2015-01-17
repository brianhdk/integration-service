using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using NHibernate;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Database.PetaPoco;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model.Web
{
    public class LatestTasksController : ApiController
	{
		private readonly IDbFactory _factory;

	    public LatestTasksController(IDbFactory factory)
	    {
		    _factory = factory;
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

			using (IDb db = _factory.OpenDatabase())
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


			using (IDb db = _factory.OpenDatabase())
			{
				taskNames = db.Query<string>(query).ToList();
			}

			return Request.CreateResponse(HttpStatusCode.OK, taskNames);
		}
    }
}
