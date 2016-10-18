using System.Collections.Generic;
using System.Web.Http;
using Examples.LiteServer.Tasks;
using Vertica.Integration.Model;

namespace Examples.LiteServer.Controllers
{
    public class ImportProductsController : ApiController
    {
        private readonly ITaskRunner _taskRunner;
        private readonly ITaskFactory _taskFactory;

        public ImportProductsController(ITaskRunner taskRunner, ITaskFactory taskFactory)
        {
            _taskFactory = taskFactory;
            _taskRunner = taskRunner;
        }

        public IHttpActionResult Get(string filePath)
        {
            ITask importProductsTask = _taskFactory.Get<ImportProductsTask>();

            var arguments = new Arguments(new KeyValuePair<string, string>("File", filePath));

            TaskExecutionResult result = _taskRunner.Execute(importProductsTask, arguments);

            return Ok(result);
        }
    }
}