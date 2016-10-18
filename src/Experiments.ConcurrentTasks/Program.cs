using System;
using Hangfire;
using Hangfire.SqlServer;
using Vertica.Integration;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Hangfire;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Model;

namespace Experiments.ConcurrentTasks
{
    class Program
    {
        static void Main(string[] args)
        {
            var db = ConnectionString.FromText("Server=.\\SQLExpress;Database=IS_ConcurrentTasks;Trusted_Connection=True;");

            using (IApplicationContext context = ApplicationContext.Create(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .Connection(db)))
                .UseLiteServer(server => server
                    .AddFromAssemblyOfThis<Program>())
                //.UseHangfire(hangfire => hangfire
                //    .AddFromAssemblyOfThis<Program>()
                //    .AddToLiteServer()
                //    .Configuration(configuration => configuration
                //        .UseSqlServerStorage(db, new SqlServerStorageOptions
                //        {
                //            QueuePollInterval = TimeSpan.FromSeconds(1),
                //        })))
                .Tasks(tasks => tasks
                    .AddFromAssemblyOfThis<Program>()
                    .MaintenanceTask()
                    .ConcurrentTaskExecution(concurrentTaskExecution => concurrentTaskExecution.AddFromAssemblyOfThis<Program>()))
                .AddCustomInstaller(Install.ByConvention.AddFromAssemblyOfThis<Program>())))
            {
                var shutdown = context.Resolve<IShutdown>();
                var factory = context.Resolve<ITaskFactory>();
                var runner = context.Resolve<ITaskRunner>();
                var liteServer = context.Resolve<ILiteServerFactory>();

                // migrate first
                runner.Execute(factory.Get<MigrateTask>());

                runner.Execute(factory.Get<SynchronousOnlyTask>());

                //runner.Execute(factory.Get<ConcurrentExecutableTask>());
                //runner.Execute(factory.Get<SynchronousOnlyTask>());

                //RecurringJob.AddOrUpdate<ITaskByNameRunner>(nameof(ConcurrentExecutableTask), x => x.Run(nameof(ConcurrentExecutableTask)), Cron.Minutely);
                //RecurringJob.AddOrUpdate<ITaskByNameRunner>(nameof(SynchronousOnlyTask), x => x.Run(nameof(SynchronousOnlyTask)), Cron.Minutely);

                using (liteServer.Create())
                {
                    shutdown.WaitForShutdown();
                }

                //maintenance last
                //runner.Execute(factory.Get<MaintenanceTask>());
            }
        }
    }
}
