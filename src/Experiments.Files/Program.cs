using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Infrastructure.Extensions;

namespace Experiments.Files
{
	class Program
	{
		static void Main()
		{
			using (IApplicationContext context = ApplicationContext.Create(application => application
				.Database(database => database.DisableIntegrationDb())
				.Tasks(tasks => tasks.Clear())
				.Hosts(hosts => hosts.Clear())
				.UseLiteServer(server => server.AddFromAssemblyOfThis<Program>())))
			{
				context.Execute(typeof (LiteServerHost).HostName());
			}
		}
	}
}
