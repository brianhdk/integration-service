using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Portal.Controllers
{
    public class TaskExecutionDetailsController : ApiController
    {
        private readonly IDbFactory _dbFactory;

        public TaskExecutionDetailsController(IDbFactory dbFactory)
		{
			_dbFactory = dbFactory;
		}

	    public HttpResponseMessage Get(int id)
		{
            var query = @"
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
  FROM [IC_PJ_Integration].[dbo].[TaskLog]
  WHERE [Id] = @0 OR TaskLog_Id = @0
";

            IEnumerable<TaskLog> taskLogs;

            using (var db = _dbFactory.OpenDatabase())
            {
                taskLogs = db.Query<TaskLog>(query, id).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, taskLogs);
        }
    }
}
