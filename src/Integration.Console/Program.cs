using Vertica.Integration.Experiments;
using Vertica.Integration.Portal;

namespace Vertica.Integration.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (ApplicationContext context = ApplicationContext.Create(application => application
                .UsePortal()
                //.RegisterTasks()
                .RegisterMigrations()
                //.TestEventLogger()
                //.TestTextFileLogger()
                //.TestPaymentService()
                .TestMonitorTask()
                .Void()))
			{
                context.Execute(args);
			}
		}
	}
}