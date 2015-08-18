using System.Diagnostics;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Owin;
using Vertica.Integration;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.WebApi;

namespace Integration.WebApi.SignalR.SmokeTester2
{
	class Program
	{
		static void Main(string[] args)
		{
			using (IApplicationContext context = ApplicationContext.Create(application => application
				.Database(database => database.DisableIntegrationDb())
				.AddCustomInstaller(new ConventionInstaller().AddFromAssemblyOfThis<Program>())
				.UseWebApi(webApi => webApi
					.Clear().AddFromAssemblyOfThis<Program>()
					.HttpServer(httpServer => httpServer.Configure(configuration =>
					{
						configuration.Http.Routes.IgnoreRoute("signalR", "signalr/{*pathInfo}");

						var hubConfiguration = new HubConfiguration
						{
							EnableDetailedErrors = true
						};

						configuration.App.MapSignalR(hubConfiguration);

						//app.Properties["host.TraceOutput"] = Console.Out;

						GlobalHost.TraceManager.Switch.Level = SourceLevels.Warning;
						GlobalHost.HubPipeline.AddModule(new MyErrorModule());
						
						// Castle.Windsor should locate all Hubs - in order to create instances.
						// 
						
					})))))
			{
				context.Execute("WebApiHost", "url:http://localhost:8400");
			}
		}
	}
}
