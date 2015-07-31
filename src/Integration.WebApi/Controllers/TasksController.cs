using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Vertica.Integration.Model;

namespace Vertica.Integration.WebApi.Controllers
{
    public class TasksController : ApiController
    {
	    private readonly ITaskFactory _factory;
        private readonly ITaskRunner _runner;

        public TasksController(ITaskFactory factory, ITaskRunner runner)
        {
	        _factory = factory;
	        _runner = runner;
        }

		public HttpResponseMessage Get()
		{
			return Request.CreateResponse(HttpStatusCode.OK, _factory.GetAll());
		}

		[Route("tasks/{name}")]
	    public HttpResponseMessage Get(string name)
	    {
		    ITask task = GetTask(name);

			if (task == null)
				return Request.CreateResponse(HttpStatusCode.NotFound);

		    return Request.CreateResponse(HttpStatusCode.OK, task);
	    }

		[Route("tasks/{name}")]
        public HttpResponseMessage Post(string name)
        {
	        ITask task = GetTask(name);

			if (task == null)
				return Request.CreateResponse(HttpStatusCode.NotFound);

			TaskExecutionResult result = _runner.Execute(task, new Arguments(Request.GetQueryNameValuePairs().ToArray()));

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

	    private ITask GetTask(string name)
	    {
		    ITask task;
		    if (_factory.TryGet(name, out task))
			    return task;

		    return null;
	    }
    }
}