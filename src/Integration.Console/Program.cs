using Vertica.Integration.Domain.Core;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Experiments;
using Vertica.Integration.Experiments.Migrations;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Portal;

namespace Vertica.Integration.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (ApplicationContext context = ApplicationContext.Create(builder => builder
                .UsePortal()
                .Migration(migration => migration
                    .IncludeFromNamespaceOfThis<M1427839041_SetupMonitorConfiguration>(DatabaseServer.SqlServer2014, builder.DatabaseConnectionString))
                .Tasks(tasks => tasks
                    .Add<TaskExecutingTask>()
                    .AddMonitorTask(/*task => task
                        .Step<ExportElmahErrorsStep>()*/)
                    .AddMaintenanceTask(/*task => task
                        .Step<CleanUpElmahErrorsStep>()*/))))
			{
			    context.Execute(args);
			}
		}
	}
}