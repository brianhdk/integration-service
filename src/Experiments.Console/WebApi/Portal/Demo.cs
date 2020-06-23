using Owin;
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
                    .HttpServer(httpServer => httpServer.Configure(configure =>
                    {
                        configure.App.Map("/html", htmlApp => htmlApp.Run(owinContext =>
                        {
                            owinContext.Response.ContentType = "text/html";
                            return owinContext.Response.WriteAsync("<h1>Hello from HTML</h1>");
                        }));
                    }))
                    .WithPortal())
                ))
            {
                // Fires up the Portal on WebAPI on the specified URL
                context.Execute(nameof(WebApiHost), "-url:http://localhost:8154");
            }
        }
    }
}