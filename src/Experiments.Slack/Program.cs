using System;
using Hangfire;
using Hangfire.SqlServer;
using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Hangfire;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Slack;
using Vertica.Integration.WebApi;

namespace Experiments.Slack
{
	class Program
	{
		static void Main(string[] args)
		{
            using (IApplicationContext context = ApplicationContext.Create(application => application
                //.Database(database => database.IntegrationDb(integrationDb => integrationDb.Disable()))
                //.Logging(logging => logging.Disable())
                .Database(database => database
                    .IntegrationDb(integrationDb => integrationDb
                        .PrefixTables("IntegrationService_")))
                .UseHangfire(hangfire => hangfire
                    .AddToLiteServer()
                    .Configuration(configuration => configuration
                        .UseSqlServerStorage(ConnectionString.FromName("IntegrationDb"), new SqlServerStorageOptions
                        {
                            QueuePollInterval = TimeSpan.FromSeconds(15)
                        })
                    )
                )
                .UseWebApi(webApi => webApi
                    .AddToLiteServer()
                    .HttpServer(httpServer => httpServer.Configure(configure =>
                    {
                        configure.App.UseHangfireDashboard("/hangfire");
                    })))
                .UseLiteServer(liteServer => liteServer
                    .AddFromAssemblyOfThis<Program>())
                //.UseSlack(slack => slack
                //    .AttachToConsoleWriter()
                //    .AddToLiteServer()
                //    .MessageHandlers(messageHandlers => messageHandlers.AddFromAssemblyOfThis<Program>())
                //    .BotCommands(botCommands => botCommands.AddFromAssemblyOfThis<Program>()))
                ))
            {
                context.Execute(args);
            }
		}
	}
}