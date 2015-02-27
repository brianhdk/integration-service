using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database.Dapper;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Portal.Controllers
{
    public class RunningTasksController : ApiController
    {
        private readonly IDapperProvider _dappper;

        public RunningTasksController(IDapperProvider dappper)
        {
            _dappper = dappper;
        }

        public HttpResponseMessage Get()
        {
            string sql = string.Format(@"
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
");

            IEnumerable<TaskLog> tasks;

            using (IDapperSession session = _dappper.OpenSession())
            {
                tasks = session.Query<TaskLog>(sql).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, tasks);
        }
    }
}