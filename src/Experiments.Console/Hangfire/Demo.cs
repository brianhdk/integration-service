using System;
using System.Threading;
using Experiments.Console.Hangfire.Migrations;
using Hangfire;
using Hangfire.SqlServer;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Hangfire;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Portal;
using Vertica.Integration.WebApi;

namespace Experiments.Console.Hangfire
{
    public static class Demo
    {
        public static void Run()
        {
            var db = ConnectionString.FromText(@"Integrated Security=SSPI;Data Source=.\SQLExpress;Database=IntegrationServiceDemo_Hangfire");

            using (var context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .PrefixTables("IntegrationService.")
                        .Connection(db)))
                .Migration(migration => migration
                    // These migrations will be stored in IntegrationDb
                    .AddFromNamespaceOfThis<M1_SetupRecurringJobs>())
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
                        .UseSqlServerStorage(db, new SqlServerStorageOptions
                        {
                            QueuePollInterval = TimeSpan.FromSeconds(5)
                        })
                    )
                )
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
                        // Running MigrateTask ensures DB schema is up to date.
                        .RunMigrateTask()))))
            {
                // Fires up the LiteServer - which includes Hangfire and WebAPI
                context.Execute(nameof(LiteServerHost));

                // You can now browse to http://localhost:8154/hangfire to see the Hangfire dashboard
            }
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
            //BackgroundJob.Enqueue<ISomeInterfaceMissingImplementation>(x => x.CallMeAndHangfireWillThrowException());

            return context.Wait(TimeSpan.FromSeconds(7));
        }
    }

    public interface ISomeInterfaceMissingImplementation
    {
        void CallMeAndHangfireWillThrowException();
    }
}