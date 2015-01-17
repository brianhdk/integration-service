using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model.Web
{
    public class LatestTasksController : ApiController
	{
		private readonly IDbFactory _dbFactory;

	    public LatestTasksController(IDbFactory dbFactory)
	    {
		    _dbFactory = dbFactory;
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

			using (IDb db = _dbFactory.OpenDatabase())
			{
                tasks = db.Query<TaskLog>(query).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, tasks);
        }
    }
}
