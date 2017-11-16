using System;
using System.Collections.Generic;
using System.Threading;
using Castle.MicroKernel;
using Experiments.Console.Hangfire.Migrations;
using Hangfire;
using Hangfire.MemoryStorage;
using Hangfire.SqlServer;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Hangfire;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.Features;
using Vertica.Integration.Model;
using Vertica.Integration.Portal;
using Vertica.Integration.WebApi;

namespace Experiments.Console.Hangfire
{
    public static class Demo
    {
        public static void Run()
        {
            var db = ConnectionString.FromText(@"Integrated Security=SSPI;Data Source=.\SQLExpress;Database=IntegrationService_Hangfire");

            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        //.Disable()
                        .PrefixTables("IntegrationDb_")
                        .Connection(db)))
                .Migration(migration => migration
                    // These migrations will be stored in IntegrationDb
                    .AddFromNamespaceOfThis<M1_SetupRecurringJobs>("CustomMigrations"))
                .Tasks(tasks => tasks
                    // clear all detault tasks ...
                    .Clear()
                    // ... except for the MigrateTask
                    .Task<MigrateTask>()
                    // also register custom tasks
                    .Task<MyTask>()
                    .Task<MyTaskWithProgressBar>())
                .Services(services => services
                    .Advanced(advanced => advanced
                        .Register<IHangfireJob, HangfireJob>()
                        // We'll override where to read RuntimeSettings from - just for demo purposes
                        // It's recommended to configure these in the app.config instead - which is the default behaviour
                        .Register<IRuntimeSettings>(kernel => new InMemoryRuntimeSettings()
                            // Specify which URL WebAPI should listen on.
                            .Set("WebApi.Url", "http://localhost:8154"))))
                // This will setup Hangfire
                .UseHangfire(hangfire => hangfire
                    .EnableConsole(console => console
                        .WithOptions(options => options
                            .ExpireIn = TimeSpan.FromDays(1)))
                    .Configuration(configuration => configuration
                        // Use InMemory storage - see Migration hack further below
                        .UseMemoryStorage(new MemoryStorageOptions())
                        // Your can also use SqlServerStorage - make sure that the database is created before running the code
                        //.UseSqlServerStorage(db, new SqlServerStorageOptions
                        //{
                        //    QueuePollInterval = TimeSpan.FromSeconds(5)
                        //})
                    ))
                // We'll also setup WebAPI so we can host the Hangfire Dashboard
                .UseWebApi(webApi => webApi
                    .WithPortal()
                    .HttpServer(httpServer => httpServer.Configure(configure =>
                    {
                        configure.App.UseHangfireDashboard("/hangfire", new DashboardOptions
                        {
                            Authorization = new[] { new AllowAllAuthorizationFilter() }
                        });
                    })))
                // Finally we'll include Hangfire and WebAPI in the LiteServer feature - so we can host both at the same time
                .UseLiteServer(liteServer => liteServer
                    .AddWebApi()
                    .AddHangfire()
                    // We'll add something that runs in the background by LiteServer, that adds fire-and-forget jobs to Hangfire
                    .AddWorker<CreateFireAndForgetJobsWorker>()
                    .AddWorker<ForceExceptionWorker>()
                    .OnStartup(startup => startup
                        // The startup-method below is a hack due to the InMemory storage provider of Hangfire
                        .Add(ClearExistingMigrationsHack)
                        // Running MigrateTask ensures DB schema is up to date.
                        .RunMigrateTask()))))
            {
                // Fires up the LiteServer - which includes Hangfire and WebAPI
                context.Execute(nameof(LiteServerHost));

                // You can now browse to http://localhost:8154/hangfire to see the Hangfire dashboard
            }
        }

        private static void ClearExistingMigrationsHack(IKernel kernel)
        {
            // Since we're running an InMemory storage, we need to rollback our custom migration (M1_SetupRecurringJobs)
            //  - On any "real" project you'll be using a persistent storage provider, e.g. SQL - so this hack will not be necessary.

            var taskRunner = kernel.Resolve<ITaskRunner>();
            var taskFactory = kernel.Resolve<ITaskFactory>();
            var featureToggler = kernel.Resolve<IFeatureToggler>();

            //if (featureToggler.IsDisabled<DbLogger>())

            taskRunner.Execute(taskFactory.Get<MigrateTask>(), new Vertica.Integration.Model.Arguments(
                new KeyValuePair<string, string>("names", "CustomMigrations"),
                new KeyValuePair<string, string>("action", "rollback")));
        }
    }

    public class CreateFireAndForgetJobsWorker : IBackgroundWorker
    {
        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            var message = $"A message from the \"Fire and forget\"-worker (#{context.InvocationCount}) - containing {{json}}";

            BackgroundJob.Enqueue<IHangfireJob>(x => x.WriteMessageToTheConsoleWriter(message));

            const string anotherMessage = "A different message from the \"Fire and forget\"-worker (#{0}) - containing string format";

            BackgroundJob.Enqueue<IHangfireJob>(x => x.WriteMessageToTheConsoleWriter(anotherMessage, context.InvocationCount));

            return context.Wait(TimeSpan.FromSeconds(10));
        }
    }

    public class ForceExceptionWorker : IBackgroundWorker
    {
        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            BackgroundJob.Enqueue<ISomeInterfaceMissingImplementation>(x => x.CallMeAndHangfireWillThrowException());

            return context.Wait(TimeSpan.FromSeconds(7));
        }
    }

    public interface ISomeInterfaceMissingImplementation
    {
        void CallMeAndHangfireWillThrowException();
    }
}