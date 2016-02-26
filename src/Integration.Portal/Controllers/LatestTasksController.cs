using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Portal.Models;

namespace Vertica.Integration.Portal.Controllers
{
    public class LatestTasksController : ApiController
    {
        private readonly IDbFactory _db;

        public LatestTasksController(IDbFactory db)
        {
            _db = db;
        }

        public HttpResponseMessage Get(int count)
        {
            string sql =
	            $@"
SELECT TOP {count}
	[Id],
	[TaskName],
	[StepName],
	[Message],
	[TimeStamp]
FROM [TaskLog]
WHERE Type = 'T'
ORDER BY timestamp DESC";

            IEnumerable<TaskLogModel> tasks;

            using (IDbSession session = _db.OpenSession())
            {
                tasks = session.Query<TaskLogModel>(sql).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, tasks);
        }
    }
}
