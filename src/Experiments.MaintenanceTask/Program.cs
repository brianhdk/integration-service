using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Castle.Core.Internal;
using Castle.MicroKernel;
using Vertica.Integration;
using Vertica.Integration.ConsoleHost;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Windows;
using Vertica.Integration.Model.Hosting;
using Vertica.Integration.Model.Hosting.Handlers;
using Vertica.Integration.WebApi;
using Vertica.Utilities_v4;

namespace Experiments.MaintenanceTask
{
    public static class IntegrationStartup
    {
        public static void Run(string[] args, Action<ApplicationConfiguration> application = null)
        {
            using (IApplicationContext context = ApplicationContext.Create(cfg => cfg
                .UseConsoleHost()
                .Change(application)))
            {
                context.Execute(args);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //Debugger.Launch();

            //var handler = new WindowsServiceHandler(new InMemoryRuntimeSettings(), new WindowsFactory());

            //string name = "IntegrationServiceExperimentsMaintenanceTaskLiteServerHost";
            //var handle = new HandleAsWindowsService(name, name, name, () =>
            //{
            //    Vertica.Integration.DeleteMeLogger.WriteAllText($"program-onstartfactory");
            //    System.Threading.Thread.Sleep(1000);

            //    return new DisposableAction(() =>
            //    {
            //        Vertica.Integration.DeleteMeLogger.WriteAllText($"program-dispose");
            //        System.Threading.Thread.Sleep(1000);
            //    });
            //});

            //handler.Handle(new ArgumentsParser().Parse(args), handle);

            //return;

            IntegrationStartup.Run(args, application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Disable()))
                .Logging(logging => logging
                    .TextFileLogger())
                .UseWebApi(webApi => webApi
                    .AddToLiteServer())
                .UseLiteServer(liteServer => liteServer
                    .AddWorker<SomeWorker>()
                    .OnStartup(startup => startup.Add(SomeHeavyStartup))
                    .OnShutdown(shutdown => shutdown.Add(SomeHeavyShutdown))));

            return;

            using (IApplicationContext context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                //        .Connection(ConnectionString.FromText("Server=.\\SQLExpress;Database=IS_MaintenanceTask;Trusted_Connection=True;"))
                        .Disable()
                ))
                .UseLiteServer(liteServer => liteServer
                    .OnStartup(startup => startup.Add(SomeHeavyStartup))
                    .AddWorker<SomeWorker>())
                //.UseMongoDb(mongoDB => mongoDB
                //    // http://docs.mongodb.org/manual/reference/connection-string/#connections-connection-options
                //    .DefaultConnection(ConnectionString.FromText("mongodb://localhost"))
                //    //.AddConnection(new CustomMongoDb(ConnectionString.FromText("mongodb://localhost")))
                //)
                //.UseUCommerce(uCommerce => uCommerce
                //    .Connection(ConnectionString.FromText("Integrated Security=SSPI;Data Source=hoka-sql01.vertica.dk;Database=Hoka_Sitecore_uCommerce")))
                ////.Migration(migration => migration.AddUCommerceFromNamespaceOfThis<M1_UCommerce>(DatabaseServer.SqlServer2014))
                //.Tasks(tasks => tasks
                //    .MaintenanceTask(m => m
                //        //.IncludeLogRotator()
                //        //.IncludeLogRotator<CustomMongoDb>()
                //        .IncludeUCommerce()
                //    )
                //)
            ))
            {
                //var factory = context.Resolve<ITaskFactory>();
                //var runner = context.Resolve<ITaskRunner>();

                //var mongoDb = context.Resolve<IMongoDbClientFactory>();
                //var rotator = context.Resolve<ILogRotatorCommand>();

                //rotator.Execute(mongoDb.Client);


                //var configuration = context.Resolve<IConfigurationService>();

                // migrate first
                //runner.Execute(factory.Get("MigrateTask"));

                // do this in a migration
                //var maintenanceConfiguration = configuration.Get<MaintenanceConfiguration>();

                //maintenanceConfiguration.ArchiveFolders.Clear();

                //maintenanceConfiguration.ArchiveFolders.AddOrUpdate("UmbracoLogFiles", (folder, handler) =>
                //{
                //    folder.Path = @"c:\\tmp";
                //    folder.ArchiveOptions.ExpiresAfter(TimeSpan.FromDays(365));

                //    return handler.FilesOlderThan(TimeSpan.FromDays(-1), "b.txt");
                //});

                //configuration.Save(maintenanceConfiguration, "BHK");

                // run the maintenance task
                //runner.Execute(factory.Get<Vertica.Integration.Domain.Core.MaintenanceTask>());
            }
        }

        private class SomeWorker : IBackgroundWorker
        {
            public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
            {
                if (context.InvocationCount == 5)
                    return context.Exit();

                File.WriteAllText($@"c:\tmp\integrationservice\worker-{context.InvocationCount}.txt", string.Empty);

                return context.Wait(TimeSpan.FromSeconds(5));
            }
        }

        private static void SomeHeavyStartup(IKernel kernel)
        {
            //Directory.EnumerateFiles(@"C:\tmp\integrationservice").ForEach(File.Delete);
            //Debugger.Launch();

            File.WriteAllText(@"c:\tmp\integrationservice\1-startup.txt", string.Empty);
            Thread.Sleep(TimeSpan.FromSeconds(10));
            File.WriteAllText(@"c:\tmp\integrationservice\2-startup.txt", string.Empty);
        }

        private static void SomeHeavyShutdown(IKernel kernel)
        {
            //Debugger.Launch();
            //File.WriteAllText(@"c:\tmp\integrationservice\1-shutdown.txt", string.Empty);
            //Thread.Sleep(TimeSpan.FromSeconds(60));
            //File.WriteAllText(@"c:\tmp\integrationservice\2-shutdown.txt", string.Empty);
        }

        private class CustomMongoDb : Vertica.Integration.MongoDB.Infrastructure.Connection
        {
            public CustomMongoDb(ConnectionString connectionString) : base(connectionString)
            {
            }
        }
    }
}