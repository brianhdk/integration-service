using Vertica.Integration.Azure;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Experiments;
using Vertica.Integration.Experiments.Migrations;
using Vertica.Integration.Infrastructure;
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
                //.Logger(logger => logger.Use<VoidLogger>())
                //.Database(database => database.Change(x => x.ConnectionString = null))
                .Database(database => database.DisableIntegrationDb())
                .Tasks(tasks => tasks.Clear().Task<TaskOutputtingHi>())))
                //.UseAzure(azure => azure
                //    .ReplaceArchiveWithBlobStorage(ConnectionString.FromName("AzureBlobStorage.Archive")))))
                //.UsePortal()
                //.Logger(logger => logger.Use<VoidLogger>())
                //.Database(db => db
                //    .AddConnection(new CustomDb(builder.DatabaseConnectionString)))
                //.Tasks(tasks => tasks
                //    .AddFromAssemblyOfThis<TaskOutputtingHi>()
                //    .MonitorTask(task => task
                //        //.IncludeElmah()
                //        .Step<XssTestingStep>())
                //    .MaintenanceTask(/*task => task
                //        .Step<CleanUpElmahErrorsStep>()*/))
                //.Migration(migration => migration
                //    .AddFromNamespaceOfThis<M1427839041_SetupMonitorConfiguration>())))
			{
			    context.Execute(args);
			}
		}
	}
}