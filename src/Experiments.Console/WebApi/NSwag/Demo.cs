using System.Web.Http;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.WebApi;
using Vertica.Integration.WebApi.NSwag;

namespace Experiments.Console.WebApi.NSwag
{
    public static class Demo
    {
        public static void Run()
        {
            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            .Set("Environment", ApplicationEnvironment.Stage)
                            .Set("WebApi.NSwag.Disabled", bool.TrueString)
                            // Specify which URL WebAPI should listen on.
                            .Set("WebApi.Url", "http://localhost:8154"))))
                .UseLiteServer(liteServer => liteServer
                    .AddWebApi())
                .UseWebApi(webApi => webApi
                    .AddFromAssemblyOfThis<DemoApiController>()
                    .WithNSwag(nswag => nswag
                        .Title("Demo")
                        .Description("Show casing NSwag")
                        .DisableIf(condition => 
                            condition.IsProduction() || 
                            condition.IsDisabledByRuntimeSettings())
                        .AddFromAssemblyOfThis<DemoApiController>()))))
            {
                // Fires up the Portal on WebAPI
                context.Execute(nameof(WebApiHost), "-noBrowser");

                // http://localhost:8154/swagger/index.html
            }
        }
    }

    public class DemoApiController : ApiController
    {
        public IHttpActionResult Get()
        {
            return Ok("Hej");
        }
    }
}