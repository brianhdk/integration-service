using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database.Dapper;
using Vertica.Integration.Portal.Models;

namespace Vertica.Integration.Portal.Controllers
{
    public class LatestTasksController : ApiController
    {
        private readonly IDapperProvider _dapper;

        public LatestTasksController(IDapperProvider dapper)
        {
            _dapper = dapper;
        }

        public HttpResponseMessage Get(int count)
        {
            string sql = string.Format(@"
SELECT TOP {0}
	[Id],
	[TaskName],
	[StepName],
	[Message],
	[TimeStamp]
FROM [TaskLog]
WHERE Type = 'T'
ORDER BY timestamp DESC", count);

            IEnumerable<TaskLogModel> tasks;

            using (IDapperSession session = _dapper.OpenSession())
            {
                tasks = session.Query<TaskLogModel>(sql).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, tasks);
        }
    }
}
