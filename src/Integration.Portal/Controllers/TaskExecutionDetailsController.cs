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
    public class TaskExecutionDetailsController : ApiController
    {
        private readonly Lazy<IDbFactory> _db;
        private readonly IIntegrationDatabaseConfiguration _configuration;

        public TaskExecutionDetailsController(Lazy<IDbFactory> db, IIntegrationDatabaseConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public HttpResponseMessage Get(int id)
        {
            if (_configuration.Disabled)
                return Request.CreateResponse(HttpStatusCode.OK, new TaskExecutionDetailModel[0]);

            string sql = $@"
SELECT [Id]
      ,[TaskName]
      ,[StepName]
      ,[Message]
      ,[ExecutionTimeSeconds]
      ,[TimeStamp]
FROM [{_configuration.TableName(IntegrationDbTable.TaskLog)}]
WHERE ([Id] = @id OR TaskLog_Id = @id)
ORDER BY [Id] DESC";

            IEnumerable<TaskExecutionDetailModel> taskLogs;

            using (IDbSession session = _db.Value.OpenSession())
            {
                taskLogs = session.Query<TaskExecutionDetailModel>(sql, new { id }).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, taskLogs);
        }
    }
}
