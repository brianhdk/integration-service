using Vertica.Integration.Experiments;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.MongoDB;

namespace Vertica.Integration.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (ApplicationContext context = ApplicationContext.Create(application => application
                //.UsePortal()
                //.UseIIS()
                //.RegisterTasks()
                //.RegisterMigrations()
                //.TestEventLogger()
                //.TestTextFileLogger()
                //.TestPaymentService()
                //.TestMonitorTask()
                //.TestMaintenanceTask()
                .TestMongoDbTask()
                .Void()))
			{
                context.Execute(args);
			}
		}
	}
}