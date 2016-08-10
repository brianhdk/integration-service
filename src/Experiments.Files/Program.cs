using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
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
				.UseLiteServer(server => server.AddFromAssemblyOfThis<Program>())
				.UseWebApi(webApi => webApi
					.AddFromAssemblyOfThis<Program>()
					.AddToLiteServer())))
			{
				context.Execute(args);
				//context.Execute(typeof (LiteServerHost).HostName());
			}
		}
	}
}
