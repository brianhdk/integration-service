using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database.Dapper;
using Vertica.Integration.Model;

namespace Vertica.Integration.Portal.Controllers
{
    public class TaskDetailsController : ApiController
    {
        private readonly IDapperProvider _dapper;
        private readonly ITaskService _taskService;

        public TaskDetailsController(ITaskService taskService, IDapperProvider dapper)
        {
            _taskService = taskService;
            _dapper = dapper;
        }

        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK, _taskService.GetAll().OrderBy(x => x.DisplayName));
        }

        public HttpResponseMessage Get(string displayName)
        {
            ITask task = _taskService.GetByName(displayName);

            return Request.CreateResponse(HttpStatusCode.OK, task);
        }

        public HttpResponseMessage Get(string displayName, int count)
        {
            string sql = string.Format(@"
SELECT TOP {0}
	[TimeStamp]
FROM [TaskLog]
WHERE [TaskName] = '{1}' AND [Type] = 'T'
ORDER BY [TimeStamp] DESC
", count, displayName);

            IEnumerable<DateTimeOffset> lastRun;

            using (IDapperSession session = _dapper.OpenSession())
            {
                lastRun = session.Query<DateTimeOffset>(sql).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, lastRun);
        }
    }
}
