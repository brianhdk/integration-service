using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database.Dapper;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Portal.Controllers
{
    public class TaskExecutionDetailsController : ApiController
    {
        private readonly IDapperProvider _dapper;

        public TaskExecutionDetailsController(IDapperProvider dapper)
        {
            _dapper = dapper;
        }

        public HttpResponseMessage Get(int id)
        {
            string sql = @"
SELECT [Id]
      ,[Type]
      ,[TaskName]
      ,[StepName]
      ,[Message]
      ,[ExecutionTimeSeconds]
      ,[TimeStamp]
      ,[TaskLog_Id]
      ,[StepLog_Id]
      ,[ErrorLog_Id]
  FROM [dbo].[TaskLog]
  WHERE [Id] = @id OR TaskLog_Id = @taskLogId";

            IEnumerable<TaskLog> taskLogs;

            using (IDapperSession session = _dapper.OpenSession())
            {
                taskLogs = session.Query<TaskLog>(sql, new { id, taskLogId = id }).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, taskLogs);
        }
    }
}
