using System.IO;
using Vertica.Integration;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Model;

namespace Experiments.Dapper
{
	class Program
	{
		static void Main()
		{
			using (IApplicationContext context = ApplicationContext.Create(application => application
				.Database(database => database.IntegrationDb(ConnectionString.FromText("Integrated Security=SSPI;Data Source=.\\SQLEXPRESS;Database=IntegrationService_Dapper")))))
			{
				var runner = context.Resolve<ITaskRunner>();
				var factory = context.Resolve<ITaskFactory>();

				runner.Execute(factory.Get<MigrateTask>());

				var writer = context.Resolve<TextWriter>();

				using (IDbSession session = context.Resolve<IDbFactory>().OpenSession())
				{
					
				}

			}
		}
	}
}
