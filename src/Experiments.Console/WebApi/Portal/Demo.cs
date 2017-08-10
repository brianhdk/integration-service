using System;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
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
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            // Specify which URL WebAPI should listen on.
                            .Set("WebApi.Url", "http://localhost:8154"))))
                .UseLiteServer(liteServer => liteServer
                    .AddWebApi())
                .UseWebApi(webApi => webApi
                    .WithPortal())))
            {
                // Fires up the Portal on WebAPI
                context.Execute(nameof(WebApiHost), "-noBrowser");
            }
        }
    }
}