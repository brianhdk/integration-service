using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Portal.Models;

namespace Vertica.Integration.Portal.Controllers
{
    public class RunningTasksController : ApiController
    {
        private readonly Lazy<IDbFactory> _db;
        private readonly IIntegrationDatabaseConfiguration _configuration;

        public RunningTasksController(Lazy<IDbFactory> db, IIntegrationDatabaseConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public HttpResponseMessage Get()
        {
            if (_configuration.Disabled)
                return Request.CreateResponse(HttpStatusCode.OK, new TaskLogModel[0]);

            string sql = $@"
SELECT
	[Id],
	[TaskName],
	[StepName],
	[Message],
	[TimeStamp]
FROM [{_configuration.TableName(IntegrationDbTable.TaskLog)}]
WHERE (
    [Type] = N'T' AND
    [ExecutionTimeSeconds] IS NULL AND
    [ErrorLog_Id] IS NULL
)
ORDER BY [Id] DESC";

            IEnumerable<TaskLogModel> tasks;

            using (IDbSession session = _db.Value.OpenSession())
            {
                tasks = session.Query<TaskLogModel>(sql).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, tasks);
        }
    }
}