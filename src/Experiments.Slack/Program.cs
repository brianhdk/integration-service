using Vertica.Integration;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Slack;

namespace Experiments.Slack
{
	class Program
	{
		static void Main(string[] args)
		{
            using (IApplicationContext context = ApplicationContext.Create(application => application
                .Database(database => database.IntegrationDb(integrationDb => integrationDb.Disable()))
                .Logging(logging => logging.Disable())
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