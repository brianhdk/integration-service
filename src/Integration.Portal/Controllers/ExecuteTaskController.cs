using System;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Exceptions;

namespace Vertica.Integration.Portal.Controllers
{
    public class ExecuteTaskController : ApiController
    {
        private readonly ITaskFactory _factory;
        private readonly ITaskRunner _runner;

        public ExecuteTaskController(ITaskFactory factory, ITaskRunner runner)
        {
            _factory = factory;
            _runner = runner;
        }

        public HttpResponseMessage Put(string name, [FromBody]string[] arguments)
        {
            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");

            ITask task;

            try
            {
                task = _factory.GetByName(name);
            }
            catch (TaskNotFoundException ex)
            {
                throw new HttpException((int)HttpStatusCode.NotFound, "Task not found.", ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, _runner.Execute(task, arguments));
        }
    }
}