using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Owin;
using Owin;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Hangfire;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Portal;
using Vertica.Integration.Slack;
using Vertica.Integration.WebApi;
using Vertica.Integration.WebHost;
using Vertica.Utilities.Extensions.EnumerableExt;

[assembly: OwinStartup(typeof(Experiments.Website.Startup))]

namespace Experiments.Website
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // TODO: Configure WebAPI base path + route ...

            app.UseIntegrationService(application => application
                .Tasks(tasks => tasks
                    .AddFromAssemblyOfThis<Startup>())
                .Services(services => services
                    .Conventions(conventions => conventions
                        .AddFromAssemblyOfThis<Startup>()))
                .UseWebApi(webApi => webApi
                    .FromCurrentApp(app)
                    .HttpServer(httpServer => httpServer.Configure(owin =>
                    {
                        // Local or Vertica ...
                        var acceptedIps = new IpMiddleware.AcceptedIps("::1", "83.151.151.132");

                        owin.App.Use(typeof(IpMiddleware), acceptedIps);

                        owin.App.UseHangfireDashboard("/hangfire", new DashboardOptions
                        {
                            Authorization = new[] {new HangfireAllowAllAuthorizationFilter()}
                        });
                    }))
                    .WithPortal()
                    .AddFromAssemblyOfThis<Startup>())
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
                            QueuePollInterval = TimeSpan.FromHours(1)
                        })
                    )
                )
                .UseLiteServer(liteServer => liteServer
                    .AddFromAssemblyOfThis<Startup>()
                    .HouseKeeping(houseKeeping => houseKeeping
                        .Interval(TimeSpan.FromMinutes(1))
                        .OutputStatusOnNumberOfIterations(60))
                    .OnStartup(startup => startup.RunMigrateTask()))
            );

            // Will run the LiteServer feature "in the background"
            app.RunIntegrationService(nameof(LiteServerHost));
        }
    }

    public class IpMiddleware : OwinMiddleware
    {
        private readonly AcceptedIps _acceptedIps;

        public IpMiddleware(OwinMiddleware next, AcceptedIps acceptedIps) :
            base(next)
        {
            if (acceptedIps == null) throw new ArgumentNullException(nameof(acceptedIps));

            _acceptedIps = acceptedIps;
        }

        public override async Task Invoke(IOwinContext context)
        {
            var ipAddress = (string) context.Request.Environment["server.RemoteIpAddress"];

            if (!_acceptedIps.Contains(ipAddress))
            {
                await context.Response.WriteAsync("Forbidden");
                context.Response.StatusCode = 403;

                return;
            }

            await Next.Invoke(context);
        }

        public class AcceptedIps
        {
            private readonly HashSet<string> _ipAddresses;

            public AcceptedIps(params string[] ipAddresses)
            {
                _ipAddresses = ipAddresses.EmptyIfNull().ToHashSet(StringComparer.OrdinalIgnoreCase);
            }

            public bool Contains(string ip)
            {
                return _ipAddresses.Contains(ip);
            }
        }
    }
}