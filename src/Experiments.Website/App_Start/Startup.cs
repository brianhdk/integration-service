using System;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Hangfire;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Slack;
using Vertica.Integration.WebHost;

[assembly: OwinStartup(typeof(Experiments.Website.Startup))]

namespace Experiments.Website
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseIntegrationService(application => application
                .Tasks(tasks => tasks.AddFromAssemblyOfThis<Startup>())
                .Services(services => services.Conventions(conventions => conventions.AddFromAssemblyOfThis<Startup>()))
                .UseSlack(slack => slack
                    .AddToLiteServer()
                    .AttachToConsoleWriter()
                    .BotCommands(botCommands => botCommands.AddFromAssemblyOfThis<Startup>())
                )
                .UseHangfire(hangfire => hangfire
                    .AddToLiteServer()
                    .Configuration(configuration => configuration
                        .UseSqlServerStorage(ConnectionString.FromName("IntegrationDb"), new SqlServerStorageOptions
                        {
                            QueuePollInterval = TimeSpan.FromSeconds(15)
                        })
                    )
                )
                .UseLiteServer(liteServer => liteServer.AddFromAssemblyOfThis<Startup>()));

            // Will run the LiteServer feature "in the background"
            app.RunIntegrationService(nameof(LiteServerHost));

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new HangfireAllowAllAuthorizationFilter() }
            });
        }
    }
}