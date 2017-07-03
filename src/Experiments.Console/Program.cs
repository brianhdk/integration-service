using System;
using System.IO;
using System.Threading;
using Rebus.Bus;
using Rebus.Handlers;
using Rebus.Logging;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Domain.LiteServer.Servers.IO;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Model;
using ILog = Rebus.Logging.ILog;
using Task = System.Threading.Tasks.Task;

namespace Experiments.Console
{
    public class MyFileWatcher : FileWatcherRunTaskServer<MyTask>
    {
        public MyFileWatcher(ITaskFactory factory, ITaskRunner runner) : base(factory, runner)
        {
        }

        protected override DirectoryInfo PathToMonitor()
        {
            return new DirectoryInfo(@"c:\tmp\tomonitor");
        }

        public override bool ShouldRestart(RestartableContext context)
        {
            if (context.FailedCount < 10)
                return true;

            return false;
        }

        protected override string Filter => "*.txt";

        protected override void AddManualFileSystemEventArgs(DirectoryInfo path, string filter, bool includeSubDirectories, NotifyFilters notifyFilters, Action<ManualFileSystemEventArgs> adder)
        {
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var context = ApplicationContext.Create(application => application
                .Tasks(tasks => tasks.AddFromAssemblyOfThis<Program>())
                .Database(database => database.IntegrationDb(integrationDb => integrationDb.Disable()))
                .UseLiteServer(liteServer => liteServer
                    .AddFromAssemblyOfThis<Program>()
                    .HouseKeeping(houseKeeping => houseKeeping
                        .Interval(TimeSpan.FromSeconds(1))
                        .OutputStatusOnNumberOfIterations(10)))))
            {
                context.Execute(nameof(LiteServerHost));
            }
            //using (var context = ApplicationContext.Create(application => application
            //    .Database(database => database
            //        .IntegrationDb(integrationDb => integrationDb
            //            .PrefixTables("IntegrationDb_")
            //            .Connection(ConnectionString.FromText(@"Integrated Security=SSPI;Data Source=.\SQLExpress;Database=IntegrationService_Rebus"))))
            //    .UseRebus(rebus => rebus
            //        .Bus((bus, kernel) => bus
            //            .Logging(logging => logging
            //                .Use(kernel.Resolve<RebusLoggerFactory>()))
            //            .Routing(routing => routing
            //                .TypeBased()
            //                    .Map<string>("inputQueue"))
            //            .Subscriptions(subscriptions => subscriptions
            //                .UseJsonFile(@"c:\tmp\rebus\json"))
            //            .Transport(transport => transport
            //                .UseInMemoryTransport(new InMemNetwork(true), "inputQueue")
            //                //.UseSqlServer(kernel.Resolve<IDbFactory>().GetConnection().ConnectionString, "Rebus", "inputQueue")
            //                //.UseFileSystem(@"c:\tmp\rebus", "inputQueue")
            //                //.UseAzureServiceBus("Endpoint=sb://XXXXXX.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=XXXXXXX", "inputQueue")
            //            )
            //        )
            //        .Handlers(handlers => handlers
            //            .AddFromAssemblyOfThis<SomeWorker>())
            //        .AddToLiteServer())
            //    .UseLiteServer(liteServer => liteServer
            //        .OnStartup(startup => startup.RunMigrateTask())
            //        .AddWorker<SomeWorker>())
            //    .Services(services => services
            //        .Advanced(advanced => advanced
            //            .Register<RebusLoggerFactory>()))))
            //{
            //    context.Execute(nameof(LiteServerHost));
            //}
        }
    }

    internal class SomeWorker : IBackgroundWorker, IRestartable
    {
        private readonly IBus _bus;

        public SomeWorker(IBus bus)
        {
            _bus = bus;
        }

        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            context.Console.WriteLine($"Sending message from Worker {context.InvocationCount}");

            _bus.Send($"Hello {context.InvocationCount}").Wait(token);

            return context.Wait(TimeSpan.FromSeconds(5));
        }

        public bool ShouldRestart(RestartableContext context)
        {
            return false;
        }

        public override string ToString()
        {
            return nameof(SomeWorker);
        }
    }

    public class RebusLoggerFactory : IRebusLoggerFactory
    {
        private readonly IConsoleWriter _console;

        public RebusLoggerFactory(IConsoleWriter console)
        {
            _console = console;
        }

        public ILog GetLogger<T>()
        {
            return new RebusLogger(_console);
        }

        private class RebusLogger : ILog
        {
            private readonly IConsoleWriter _console;

            public RebusLogger(IConsoleWriter console)
            {
                _console = console;
            }

            public void Debug(string message, params object[] objs)
            {
                _console.WriteLine(message, objs);
            }

            public void Info(string message, params object[] objs)
            {
                _console.WriteLine(message, objs);
            }

            public void Warn(string message, params object[] objs)
            {
                _console.WriteLine(message, objs);
            }

            public void Error(Exception exception, string message, params object[] objs)
            {
                _console.WriteLine(message, objs);
            }

            public void Error(string message, params object[] objs)
            {
                _console.WriteLine(message, objs);
            }
        }
    }

    public class MyMessageHandler : IHandleMessages<string>
    {
        private readonly IConsoleWriter _writer;

        public MyMessageHandler(IConsoleWriter writer)
        {
            _writer = writer;
        }

        public Task Handle(string message)
        {
            _writer.WriteLine($"Receiving message: {message}");

            return Task.FromResult(true);
        }
    }
}
