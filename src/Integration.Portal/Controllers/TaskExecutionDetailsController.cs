using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Portal.Models;

namespace Vertica.Integration.Portal.Controllers
{
    public class TaskExecutionDetailsController : ApiController
    {
        private readonly IDbFactory _db;

        public TaskExecutionDetailsController(IDbFactory db)
        {
            _db = db;
        }

        public HttpResponseMessage Get(int id)
        {
            const string sql = @"
SELECT [Id]
      ,[TaskName]
      ,[StepName]
      ,[Message]
      ,[ExecutionTimeSeconds]
      ,[TimeStamp]
FROM [TaskLog]
WHERE ([Id] = @id OR TaskLog_Id = @id)
ORDER BY [Id] DESC";

            IEnumerable<TaskExecutionDetailModel> taskLogs;

            using (IDbSession session = _db.OpenSession())
            {
                taskLogs = session.Query<TaskExecutionDetailModel>(sql, new { id }).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, taskLogs);
        }
    }
}
