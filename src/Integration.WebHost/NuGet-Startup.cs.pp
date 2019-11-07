using Microsoft.Owin;
using Owin;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.WebApi;
using Vertica.Integration.WebHost;
using $rootnamespace$;

[assembly: OwinStartup(typeof(Startup))]

namespace $rootnamespace$
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Bootstraps Integration Service
            app.UseIntegrationService(application => application
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        // Disables the IntegrationDb entirely (optional - not recommended)
                        .Disable()
                        // Prefix the table for the IntegrationService database
                        //.PrefixTables("IntegrationService.")
                    )
                )
                .UseWebApi(webApi => webApi
                    // Initializes WebApi
                    .FromCurrentApp(app)
                    // Run "Install-Package Vertica.Integration.Portal" to setup the Integration Service Portal and uncomment the following line
                    //.WithPortal()
                    // Scans this assembly for WebApiControllers
                    .AddFromAssemblyOfThis<Startup>()
                )
                .Services(services => services
                    .Conventions(conventions => conventions
                        // Scans this assembly for conventional service registrations
                        .AddFromAssemblyOfThis<Startup>()))
                .UseLiteServer(liteServer => liteServer
                    // Scans this assembly for LiteServer background-workers and/or -servers
                    .AddFromAssemblyOfThis<Startup>()
                    //.OnStartup(startup => startup
                    //    // Runs the MigrateTask part of starting up the Integration Service
                    //    .RunMigrateTask()
                    //)
                )
            );

            // Runs the LiteServerHost in the background
            app.RunIntegrationService(nameof(LiteServerHost));
        }
    }
}