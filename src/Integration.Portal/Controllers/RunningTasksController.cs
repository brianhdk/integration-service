using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Portal.Controllers
{
    public class RunningTasksController : ApiController
    {
        private readonly IDbFactory _dbFactory;

        public RunningTasksController(IDbFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public HttpResponseMessage Get()
        {
            var query = string.Format(@"
SELECT
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
WHERE ExecutionTimeSeconds = 0
AND ErrorLog_Id IS NULL
 order by TimeStamp desc
");

            IEnumerable<TaskLog> tasks;

            using (IDb db = _dbFactory.OpenDatabase())
            {
                tasks = db.Query<TaskLog>(query).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, tasks);
        }
    }
}