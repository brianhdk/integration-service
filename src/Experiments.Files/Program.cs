using Vertica.Integration;

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
				.UseServer(server => server.AddFromAssemblyOfThis<Program>())))
			{
				context.Execute(args);
			}
		}
	}
}
