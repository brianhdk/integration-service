using Vertica.Integration.Domain.Core;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Experiments;
using Vertica.Integration.Experiments.Migrations;
using Vertica.Integration.Logging.Elmah;
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
                    .IncludeFromNamespaceOfThis<M1427839041_SetupMonitorConfiguration>())
                .Tasks(tasks => tasks
                    .AddTasksFromAssemblyOfThis<TaskOutputtingHi>()
                    .AddMonitorTask(task => task.IncludeElmah().Skip<ExportElmahErrorsStep>())
                    .AddMaintenanceTask(/*task => task
                        .Step<CleanUpElmahErrorsStep>()*/))))
			{
			    context.Execute(args);
			}
		}
	}
}