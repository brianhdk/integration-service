using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Castle.DynamicProxy;
using Hangfire;
using Hangfire.Common;
using Hangfire.Console;
using Hangfire.Server;
using Hangfire.SqlServer;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Domain.LiteServer.Servers.IO;
using Vertica.Integration.Hangfire;
using Vertica.Integration.Infrastructure;
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
                    .Configuration(configuration => configuration
                        .UseSqlServerStorage(ConnectionString.FromName("IntegrationDb"), new SqlServerStorageOptions
                        {
                            QueuePollInterval = TimeSpan.FromSeconds(15)
                        })
                        //.UseColouredConsoleLogProvider()
                        .UseConsole()
                        .UseFilter(new GetContext())
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
                        .AddFromAssemblyOfThis<Program>())
                    .Interceptors(interceptors => interceptors
                        .InterceptService<IConsoleWriter, HangfireConsoleWriterInterceptor>()
                        .InterceptService<ILogger, HangfireLoggerInterceptor>()
                    )
                )
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

    public interface IJobRunner
    {
        void Run(string fileName);
    }

    public class JobRunner : IJobRunner
    {
        private readonly ITaskFactory _factory;
        private readonly ITaskRunner _taskRunner;

        public JobRunner(ITaskFactory factory, ITaskRunner taskRunner)
        {
            _factory = factory;
            _taskRunner = taskRunner;
        }

        public void Run(string fileName)
        {
            _taskRunner.Execute(
                _factory.Get<SomeTask>(), 
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

            file.Delete();
        }

        public override string Description => "Test";
    }

    public class AddHangfireJob : FileWatcherServer
    {
        protected override DirectoryInfo PathToMonitor()
        {
            return new DirectoryInfo(@"c:\tmp\hangfire");
        }

        protected override void ProcessFile(FileInfo file, FileSystemEventArgs args)
        {
            BackgroundJob.Enqueue<IJobRunner>(x => x.Run(file.FullName));
        }

        protected override void ProcessDirectory(DirectoryInfo directory, FileSystemEventArgs args)
        {
        }

        protected override void AddManualFileSystemEventArgs(DirectoryInfo path, string filter, bool includeSubDirectories, NotifyFilters notifyFilters, Action<ManualFileSystemEventArgs> adder)
        {
        }
    }

    public class GetContext : JobFilterAttribute, IServerFilter
    {
        [ThreadStatic]
        private static PerformingContext _context;

        public static PerformingContext Context => _context;

        public void OnPerforming(PerformingContext filterContext)
        {
            _context = filterContext;
        }

        public void OnPerformed(PerformedContext filterContext)
        {
        }
    }

    internal class HangfireLoggerInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            if (invocation.Method.Name.Equals(nameof(ILogger.LogEntry)))
            {
                //var logEntry = invocation.Arguments[0] as LogEntry;

                //if (logEntry != null)
                //{
                //    GetContext.Context.WriteLine(logEntry.ToString());
                //}
            }
            else
            {
                var errorLog = invocation.ReturnValue as ErrorLog;

                if (errorLog != null)
                {
                    GetContext.Context.SetTextColor(ConsoleTextColor.Red);
                    GetContext.Context.WriteLine(" ID: {0}", errorLog.Id);
                    GetContext.Context.ResetTextColor();
                }
            }
        }
    }

    internal class HangfireConsoleWriterInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            var message = (string)invocation.Arguments.FirstOrDefault();

            if (message != null)
            {
                var args = (object[])invocation.Arguments.ElementAtOrDefault(1);

                if (args != null)
                    message = string.Format(message, args);

                PerformingContext context = GetContext.Context;
                context.WriteLine($" {message}");
            }
        }
    }
}