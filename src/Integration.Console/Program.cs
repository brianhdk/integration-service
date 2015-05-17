using Vertica.Integration.Domain.Core;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Experiments;
using Vertica.Integration.Experiments.Migrations;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Portal;

namespace Vertica.Integration.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (ApplicationContext context = ApplicationContext.Create(builder => builder
                .UsePortal()
                .Database(db => db
                    .Change(x => x.ConnectionString = ConnectionString.FromName("IntegrationDb"))
                    .AddConnection(new CustomDb(builder.DatabaseConnectionString)))
                .Tasks(tasks => tasks
                    .AddFromAssemblyOfThis<TaskOutputtingHi>()
                    .MonitorTask(task => task
                        .Step<XssTestingStep>())
                    .MaintenanceTask(/*task => task
                        .Step<CleanUpElmahErrorsStep>()*/))
                .Migration(migration => migration
                    .AddFromNamespaceOfThis<M1427839041_SetupMonitorConfiguration>())))
			{
			    context.Execute(args);
			}
		}
	}
}