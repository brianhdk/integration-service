using System.Web;
using System.Web.Http;
using Microsoft.Owin;
using Vertica.Integration;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Model;
using Vertica.Integration.WebHost;

namespace Experiments.Website.Controllers
{
    public class RunTaskController : ApiController
    {
        public IHttpActionResult Get()
        {
            IApplicationContext integrationService = OwinContext.GetIntegrationService();

            var runner = integrationService.Resolve<ITaskRunner>();
            var factory = integrationService.Resolve<ITaskFactory>();

            TaskExecutionResult result = runner.Execute(factory.Get<WriteDocumentationTask>());

            return Ok(result);
        }

        private IOwinContext OwinContext => HttpContext.Current.GetOwinContext();
    }
}