using System.Web.Http;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Model;

namespace Experiments.Website.Controllers
{
    public class RunTaskController : ApiController
    {
        private readonly ITaskRunner _runner;
        private readonly ITaskFactory _factory;

        public RunTaskController(ITaskRunner runner, ITaskFactory factory)
        {
            _runner = runner;
            _factory = factory;
        }

        public IHttpActionResult Get()
        {
            TaskExecutionResult result = _runner.Execute(_factory.Get<WriteDocumentationTask>());

            return Ok(result);
        }
    }
}