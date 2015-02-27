using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model.Web
{
	public class TaskDetailsController : ApiController
	{
		private readonly IDbFactory _dbFactory;
		private readonly ITaskService _taskService;

		public TaskDetailsController(IDbFactory dbFactory, ITaskService taskService)
		{
			_dbFactory = dbFactory;
			_taskService = taskService;
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
			var query = string.Format(@"
SELECT TOP {0} *
FROM [TaskLog]
WHERE TaskName = '{1}' AND Type = 'T'
ORDER BY timestamp DESC
", count, displayName);

			IEnumerable<TaskLog> tasks;

			using (IDb db = _dbFactory.OpenDatabase())
			{
				tasks = db.Query<TaskLog>(query).ToList();
			}

			return Request.CreateResponse(HttpStatusCode.OK, tasks);
		}
	}
}
