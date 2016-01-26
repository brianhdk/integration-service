using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Owin;
using Microsoft.Owin.Diagnostics;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using Vertica.Integration.Experiments;
using Vertica.Integration.Experiments.Migrations.CustomDb;
using Vertica.Integration.Experiments.SignalR;
using Vertica.Integration.Experiments.WebApi;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Model;
using Vertica.Integration.Perfion;
using Vertica.Integration.WebApi;
using Vertica.Integration.WebApi.SignalR;

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
				//.Advanced(advanced => advanced.Register(() => TextWriter.Null))
				//.UseWebApi(webApi => webApi
				//	.AddFromAssemblyOfThis<TestController>()
				//	.HttpServer(httpServer => httpServer.Configure(configurer =>
				//	{
				//		configurer.App.UseFileServer(new FileServerOptions
				//		{
				//			RequestPath = PathString.Empty,
				//			FileSystem = new PhysicalFileSystem(@"..\..\..\Integration.Experiments\SignalR\Html")
				//		});

				//		configurer.App.UseErrorPage(new ErrorPageOptions { ShowSourceCode = false });

				//		configurer.App.Use((ctx, next) =>
				//		{
				//			if (ctx.Request.Path.Value == "/fail")
				//				throw new Exception("Failed");
				//			if (ctx.Request.Path.Value == "/write")
				//				return ctx.Response.WriteAsync("Yo!");

				//			return next.Invoke();
				//		});
				//	}))
				//	.WithSignalR(signalR => signalR
				//		//.SkipTraceConfiguration()
				//		//.TraceLevel(SourceLevels.All)
				//		.AddFromAssemblyOfThis<ChatHub>())
				//)
				//.Logging(logging => logging.Disable())
				//.Logging(logging => logging.TextWriter())
				//.AddCustomInstaller(Install.Service<ChatHub.RandomChatter>())
                //.UseIIS()
				.Fast()
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
				//.TestBizTalkTracker()
				//.Migration(migration => migration
				//	.AddFromNamespaceOfThis<M1431659519_CustomDbVoid>("CustomDb"))
				.Tasks(tasks => tasks.Task<FtpTesterTask>().Task<SomeTask>())
				.UsePerfion(perfion => perfion
					.ServiceClient(serviceClient => serviceClient
						.Change(x => x.SendTimeout = TimeSpan.MaxValue)))
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