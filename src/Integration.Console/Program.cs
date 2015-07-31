using Vertica.Integration.Experiments;
using Vertica.Integration.Model;

namespace Vertica.Integration.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (IApplicationContext context = ApplicationContext.Create(application => application
				//.UseWebApi(webApi => webApi.WithPortal())
                //.UseIIS()
				.Fast()
				//.Tasks(tasks => tasks
				//	//.Task<Task1.SameNameTask>()
				//	.Task<Task2.SameNameTask>())
                //.RegisterTasks()
                //.RegisterMigrations()
                //.TestEventLogger()
				//.TestTextFileLogger()
                //.TestPaymentService()
                //.TestMonitorTask()
                //.TestMaintenanceTask()
                //.TestMongoDbTask()
				//.Hosts(hosts => hosts.Remove<WebApiHost>())
				//.RegisterMigrations()
				.TestRebus()
				.TestRavenDB()
                .Void()))
			{
                context.Execute(args);
			}
		}
	}
}

namespace Task1
{
    public class SameNameTask : Task
    {
        public override string Description
        {
            get { return "Task1"; }
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            context.Log.Message("Task1");
        }
    }
}

namespace Task2
{
    public class SameNameTask : Task
    {
        public override string Description
        {
            get { return "Task2"; }
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            context.Log.Message("Task2");
        }
    }
}