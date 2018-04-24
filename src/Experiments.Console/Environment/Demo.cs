using System;
using System.Threading;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Migrations;

namespace Experiments.Console.Environment
{
    public static class Demo
    {
        public static void Run()
        {
            var db = ConnectionString.FromText(@"Integrated Security=SSPI;Data Source=.\SQLExpress;Database=IntegrationServiceDemo_Environment");

            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .PrefixTables("IntegrationService.")
                        .Connection(db)))
                .Tasks(tasks => tasks
                    // clear all detault tasks ...
                    .Clear()
                    // ... except for the MigrateTask
                    .Task<MigrateTask>())
                // Finally we'll include Hangfire and WebAPI in the LiteServer feature - so we can host both at the same time
                .UseLiteServer(liteServer => liteServer
                    // We'll add something that runs in the background by LiteServer, that adds fire-and-forget jobs to Hangfire
                    .AddWorker<SayHelloWorker>())
                .Environment(environment => environment
                    .Customize(ApplicationEnvironment.Production, production => production
                        .Database(database => database
                            .IntegrationDb(integrationDb => integrationDb
                                // Will disable creating the database if running in production mode
                                // This is necessary when hosting the database in Azure
                                .DisableCheckExistsAndCreateDatabaseIfNotFound()))
                        .UseLiteServer(liteServer => liteServer
                            .OnStartup(startup => startup
                                .RunMigrateTask())))
                    .OverrideCurrent(ApplicationEnvironment.Production))))
            {
                // Remember to manually create the 'IntegrationServiceDemo_Environment' database before running this
                context.Execute(nameof(LiteServerHost));
            }
        }
    }

    public class SayHelloWorker : IBackgroundWorker
    {
        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            context.Console.WriteLine($"Hello #{context.InvocationCount}");

            return context.Wait(TimeSpan.FromSeconds(10));
        }
    }
}