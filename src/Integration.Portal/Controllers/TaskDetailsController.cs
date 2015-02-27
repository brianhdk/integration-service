using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database.Dapper;
using Vertica.Integration.Infrastructure.Logging;
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
            return Request.CreateResponse(HttpStatusCode.OK, _taskService.GetAll());
        }

        public HttpResponseMessage Get(string displayName)
        {
            ITask task = _taskService.GetByName(displayName);

            return Request.CreateResponse(HttpStatusCode.OK, task);
        }

        public HttpResponseMessage Get(string displayName, int count)
        {
            string sql = string.Format(@"
SELECT TOP {0} *
FROM [TaskLog]
WHERE TaskName = '{1}' AND Type = 'T'
ORDER BY timestamp DESC
", count, displayName);

            IEnumerable<TaskLog> tasks;

            using (IDapperSession session = _dapper.OpenSession())
            {
                tasks = session.Query<TaskLog>(sql).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, tasks);
        }
    }
}
