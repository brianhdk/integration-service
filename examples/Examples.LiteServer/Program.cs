using System;
using Examples.LiteServer.Infrastructure.Migrations;
using Hangfire;
using Hangfire.SqlServer;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Hangfire;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Portal;
using Vertica.Integration.WebApi;

namespace Examples.LiteServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ConnectionString integrationDb = ConnectionString.FromText("Server=.\\SQLExpress;Database=TeamMeeting_LiteServer;Trusted_Connection=True;");

            IntegrationStartup.Run(args, application => application
                .Database(database => database.IntegrationDb(integrationDb))
                .Tasks(tasks => tasks.AddFromAssemblyOfThis<Program>())
                .UseLiteServer(liteServer => liteServer
                    .AddFromAssemblyOfThis<Program>()
                    .OnStartup(startup => startup.RunMigrateTask()))
                .UseWebApi(webApi => webApi
                    .AddFromAssemblyOfThis<Program>()
                    .AddToLiteServer()
                    .WithPortal()
                    .HttpServer(httpServer => httpServer.Configure(configurer =>
                    {
                        configurer.App.UseHangfireDashboard("/hangfire");
                    }))
                )
                .UseHangfire(hangfire => hangfire
                    .Configuration(configuration => configuration
                        .UseSqlServerStorage(integrationDb, new SqlServerStorageOptions
                        {
                            QueuePollInterval = TimeSpan.FromSeconds(1)
                        })
                    )
                    .AddFromAssemblyOfThis<Program>()
                    .AddToLiteServer())
                .Migration(migration => migration.AddFromNamespaceOfThis<M1_SetupRecurringTasks>())
                .AddCustomInstaller(Install.ByConvention.AddFromAssemblyOfThis<Program>())
            );
        }
    }
}
