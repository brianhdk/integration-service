using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Vertica.Integration.Model.Web
{
    public class TaskController : ApiController
    {
        private readonly ITaskRunner _taskRunner;

        public TaskController(ITaskRunner taskRunner)
        {
            _taskRunner = taskRunner;
        }

        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK, GetCurrentContext().Task);
        }

        public HttpResponseMessage Post(HttpRequestMessage request)
        {
            WebApiHost.Context context = GetCurrentContext();

            TaskExecutionResult result = _taskRunner.Execute(context.TaskName, context.Task, context.Arguments);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        private WebApiHost.Context GetCurrentContext()
        {
            return WebApiHost.Context.Get(Configuration);
        }
    }
}