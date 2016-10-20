using System;
using Hangfire;
using Hangfire.SqlServer;
using Vertica.Integration;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Hangfire;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Logging.Elmah;
using Vertica.Integration.Portal;
using Vertica.Integration.UCommerce;
using Vertica.Integration.WebApi;

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
                        .Connection(db)
                        .PrefixTables("IntegrationService6.")))
                .UseLiteServer(server => server
                    .AddFromAssemblyOfThis<Program>()
                    .OnStartup(startup => startup.RunMigrateTask()))
                .UseWebApi(webApi => webApi
                    .AddToLiteServer()
                    .WithPortal())
                .UseUCommerce(uCommerce => uCommerce
                    .Connection(new CustomUCommerceDb()))
                .UseHangfire(hangfire => hangfire
                    .AddFromAssemblyOfThis<Program>()
                    .AddToLiteServer()
                    .Configuration(configuration => configuration
                        .UseSqlServerStorage(db, new SqlServerStorageOptions
                        {
                            QueuePollInterval = TimeSpan.FromSeconds(1),
                        })))
                .Tasks(tasks => tasks
                    .AddFromAssemblyOfThis<Program>()
                    .MaintenanceTask(task => task
                        //.IncludeElmah()
                        .IncludeUCommerce())
                    .MonitorTask(task => task
                        .IncludeElmah())
                    .ConcurrentTaskExecution(concurrentTaskExecution => concurrentTaskExecution.AddFromAssemblyOfThis<Program>()))
                .AddCustomInstaller(Install.ByConvention.AddFromAssemblyOfThis<Program>())))
            {
                var shutdown = context.Resolve<IShutdown>();
                var liteServer = context.Resolve<ILiteServerFactory>();
                
                RecurringJob.AddOrUpdate<ITaskByNameRunner>(nameof(ConcurrentExecutableTask), x => x.Run(nameof(ConcurrentExecutableTask)), Cron.Minutely);
                RecurringJob.AddOrUpdate<ITaskByNameRunner>(nameof(SynchronousOnlyTask), x => x.Run(nameof(SynchronousOnlyTask)), Cron.Minutely);

                using (liteServer.Create())
                {
                    shutdown.WaitForShutdown();
                }
            }
        }
    }
}
