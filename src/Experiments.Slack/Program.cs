using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Hangfire;
using Hangfire.Console;
using Hangfire.SqlServer;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Domain.LiteServer.Servers.IO;
using Vertica.Integration.Hangfire;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;
using Vertica.Integration.WebApi;

namespace Experiments.Slack
{
    class Program
	{
		static void Main(string[] args)
		{
            using (IApplicationContext context = ApplicationContext.Create(application => application
                //.Database(database => database.IntegrationDb(integrationDb => integrationDb.Disable()))
                //.Logging(logging => logging.Disable())
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .PrefixTables("IntegrationService_")))
                .UseHangfire(hangfire => hangfire
                    .AddToLiteServer()
                    .EnableConsole()
                    .Configuration((configuration, kernel) => configuration
                        .UseSqlServerStorage(ConnectionString.FromName("IntegrationDb"), new SqlServerStorageOptions
                        {
                            QueuePollInterval = TimeSpan.FromSeconds(15)
                        })
                    )
                )
                .Tasks(tasks => tasks
                    .AddFromAssemblyOfThis<Program>())
                .UseWebApi(webApi => webApi
                    .AddToLiteServer()
                    .HttpServer(httpServer => httpServer.Configure(configure =>
                    {
                        configure.App.UseHangfireDashboard("/hangfire");
                    })))
                .UseLiteServer(liteServer => liteServer
                    .AddFromAssemblyOfThis<Program>())
                .Services(services => services
                    .Conventions(conventions => conventions
                        .AddFromAssemblyOfThis<Program>()))
                //.UseSlack(slack => slack
                //    .AttachToConsoleWriter()
                //    .AddToLiteServer()
                //    .MessageHandlers(messageHandlers => messageHandlers.AddFromAssemblyOfThis<Program>())
                //    .BotCommands(botCommands => botCommands.AddFromAssemblyOfThis<Program>()))
                ))
            {
                context.Execute(args);
            }
		}
	}

    public class AddHangfireJob : FileWatcherServer
    {
        protected override DirectoryInfo PathToMonitor()
        {
            return new DirectoryInfo(@"c:\tmp\hangfire");
        }

        protected override void ProcessFile(FileInfo file, FileSystemEventArgs args)
        {
            BackgroundJob.Enqueue<IRunTask>(x => x.Run("SomeTask", file.FullName));
        }

        protected override void ProcessDirectory(DirectoryInfo directory, FileSystemEventArgs args)
        {
        }

        protected override void AddManualFileSystemEventArgs(DirectoryInfo path, string filter, bool includeSubDirectories, NotifyFilters notifyFilters, Action<ManualFileSystemEventArgs> adder)
        {
        }
    }

    public interface IRunTask
    {
        void Run(string taskName, string fileName);
    }

    public class RunTask : IRunTask
    {
        private readonly ITaskFactory _factory;
        private readonly ITaskRunner _taskRunner;

        public RunTask(ITaskFactory factory, ITaskRunner taskRunner)
        {
            _factory = factory;
            _taskRunner = taskRunner;
        }

        public void Run(string taskName, string fileName)
        {
            _taskRunner.Execute(
                _factory.Get(taskName), 
                new Arguments(new KeyValuePair<string, string>("FileName", fileName)));
        }
    }

    public class SomeTask : Task
    {
        public override void StartTask(ITaskExecutionContext context)
        {
            var file = new FileInfo(context.Arguments["FileName"]);

            for (int i = 0; i < 100; i++)
            {
                if (i % 10 == 0)
                    context.Log.Warning(Target.All, "Warning");

                if (i % 25 == 0)
                    context.Log.Error(Target.Service, "Some error");

                if (i == 75)
                    context.Log.Exception(new DivideByZeroException());

                context.Log.Message($"{file.Name} - {i}");
                Thread.Sleep(100);
            }

            if (file.Exists)
                file.Delete();
        }

        public override string Description => "Test";
    }
}