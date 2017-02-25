using System;
using System.Web;
using System.Web.Mvc;
using Microsoft.Owin;
using Vertica.Integration;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Model;
using Vertica.Integration.WebHost;

namespace Experiments.Website.Controllers
{
    public class HomeController : Controller
    {
        public ViewResult Index()
        {
            //return View(new TaskExecutionResult(new[] {"A"}));

            try
            {
                IApplicationContext integrationService = OwinContext.GetIntegrationService();

                var runner = integrationService.Resolve<ITaskRunner>();
                var factory = integrationService.Resolve<ITaskFactory>();

                TaskExecutionResult result = runner.Execute(factory.Get<WriteDocumentationTask>());

                return View(result);
            }
            catch
            {
                return View(new TaskExecutionResult(new[] { "A" }));
            }
        }

        private IOwinContext OwinContext => HttpContext.GetOwinContext();
    }
}