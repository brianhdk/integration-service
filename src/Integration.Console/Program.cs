using Vertica.Integration.Domain.Core;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Experiments;
using Vertica.Integration.Experiments.Migrations;
using Vertica.Integration.Infrastructure.Logging.Loggers;
using Vertica.Integration.Logging.Elmah;
using Vertica.Integration.Portal;

namespace Vertica.Integration.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (ApplicationContext context = ApplicationContext.Create(builder => builder
                //.UseAzure(azure => azure
                //    .ReplaceArchiveWithBlobStorage(ConnectionString.FromName("AzureBlobStorage.Archive")))))
                .UsePortal()
                .Logger(logger => logger.Use<VoidLogger>())
                .Database(db => db
                    .AddConnection(new CustomDb(builder.DatabaseConnectionString)))
                .Tasks(tasks => tasks
                    .AddFromAssemblyOfThis<HelloTask>()
                    .MonitorTask(task => task
                        .IncludeElmah()
                        .Step<XssTestingStep>())
                    .MaintenanceTask(task => task
                        .IncludeElmah()))
                .Migration(migration => migration
                    .AddFromNamespaceOfThis<M1427839041_SetupMonitorConfiguration>())))
			{
			    context.Execute(args);
			}
		}
	}
}