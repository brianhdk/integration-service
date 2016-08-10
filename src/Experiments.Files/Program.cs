using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Hangfire;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.WebApi;

namespace Experiments.Files
{
	class Program
	{
		static void Main(string[] args)
		{
			using (IApplicationContext context = ApplicationContext.Create(application => application
				.Database(database => database.DisableIntegrationDb())
				.Tasks(tasks => tasks.Clear())
				.Hosts(hosts => hosts.Clear())
				.UseLiteServer(liteServer => liteServer
					.AddFromAssemblyOfThis<Program>())
				.UseWebApi(webApi => webApi
					.AddFromAssemblyOfThis<Program>()
					.AddToLiteServer())
				.UseHangfire(hangfire => hangfire
					.AddFromAssemblyOfThis<Program>()
					.AddToLiteServer())))
			{
				//context.Execute(args);
				context.Execute(typeof (LiteServerHost).HostName());
			}
		}
	}
}
