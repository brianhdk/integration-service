using Vertica.Integration;
using Vertica.Integration.Portal;
using Vertica.Integration.WebApi;

namespace Experiments.Console.WebApi.Portal
{
    public static class Demo
    {
        public static void Run()
        {
            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .UseWebApi(webApi => webApi
                    .WithPortal())))
            {
                // Fires up the Portal on WebAPI on the specified URL
                context.Execute(nameof(WebApiHost), "-url:http://localhost:8154");
            }
        }
    }
}