using System.Web.Http;
using Owin;
using Vertica.Integration;
using Vertica.Integration.WebApi;

namespace Integration.WebApi.SignalR.SmokeTester
{
	class Program
	{
		static void Main(string[] args)
		{
			using (IApplicationContext context = ApplicationContext.Create(application => application
				.Database(database => database.DisableIntegrationDb())
				.UseWebApi(webApi => webApi
					.Clear().AddFromAssemblyOfThis<Program>()
					.HttpServer(httpServer => httpServer.Configure((app, config) =>
					{
						config.Routes.IgnoreRoute("signalR", "signalr/{*pathInfo}");
						app.MapSignalR();
					})))))
			{
				context.Execute("WebApiHost");
			}
		}
	}
}