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
			var tasks = _taskService.GetAll().ToList();
		    return Request.CreateResponse(HttpStatusCode.OK, tasks);
        }

	    public HttpResponseMessage Get(string displayName)
	    {
		    var task = _taskService.GetAll().FirstOrDefault(x => x.DisplayName == displayName);


			return Request.CreateResponse(HttpStatusCode.OK, task);
        }
    }
}
