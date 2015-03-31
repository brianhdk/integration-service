using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Vertica.Integration.Model.Web
{
    public class TaskController : ApiController
    {
        private readonly ITaskRunner _runner;

        public TaskController(ITaskRunner runner)
        {
            _runner = runner;
        }

        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK, GetCurrentContext().Task);
        }

        public HttpResponseMessage Post(HttpRequestMessage request)
        {
            WebApiHost.Context context = GetCurrentContext();

            TaskExecutionResult result = _runner.Execute(context.Task, context.Arguments);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        private WebApiHost.Context GetCurrentContext()
        {
            return WebApiHost.Context.Get(Configuration);
        }
    }
}