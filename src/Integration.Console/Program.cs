using Vertica.Integration.Experiments;
using Vertica.Integration.Experiments.BizTalkTracker;
using Vertica.Integration.Model;
using Vertica.Integration.Portal;
using Vertica.Integration.WebApi;

namespace Vertica.Integration.Console
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using (IApplicationContext context = ApplicationContext.Create(application => application
				//.Database(database => database.Change(db => db.ConnectionString = ConnectionString.FromName("IntegrationDb.Alternate")))
				//.Database(database => database.AddConnection(new CustomDb(ConnectionString.FromText("..."))))
				//.Logging(logging => logging.Use<ConsoleLogger>())
				//.UseWebApi(webApi => webApi
				//	.WithPortal())
				//.UseWebApi(webApi => webApi
				//	.AddFromAssemblyOfThis<TestController>()
				//	.HttpServer(httpServer => httpServer.Configure(configurer =>
				//	{
				//		configurer.App.UseFileServer(new FileServerOptions
				//		{
				//			RequestPath = PathString.Empty,
				//			FileSystem = new PhysicalFileSystem(@"..\..\..\Integration.Experiments\SignalR\Html")
				//		});
				//	}))
				//	.WithPortal()
				//	.WithSignalR(signalR => signalR.AddFromAssemblyOfThis<ChatHub>())
				//)
				//.AddCustomInstaller(Install.Service<ChatHub.RandomChatter>())
                //.UseIIS()
				//.Fast()
				//.TestAzure()
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
				//.TestRebus(args)
				//.TestRavenDb()
				.TestBizTalkTracker()
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