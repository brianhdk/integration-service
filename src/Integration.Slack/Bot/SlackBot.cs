using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SlackConnector;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Slack.Bot.Commands;

namespace Vertica.Integration.Slack.Bot
{
    public class SlackBot : ISlackBot
    {
        public SlackBot(ISlackBotCommand[] commands, ISlackConfiguration configuration, IShutdown shutdown)
        {
            if (!configuration.Enabled)
                return;

            CancellationToken token = shutdown.Token;

            Worker = Task.Run(() =>
            {
                SlackConnector.SlackConnector connector = new SlackConnector.SlackConnector();

                ISlackConnection client = connector.Connect(configuration.BotUserToken).Result;

                client.OnMessageReceived += message =>
                {
                    var context = new SlackBotCommandContext(client.Say, message);

                    var tasks = new List<Task>();

                    Parallel.ForEach(commands, command =>
                    {
                        Task task;
                        if (command.TryHandle(context, token, out task))
                            tasks.Add(task);
                    });

                    // TODO: Implement various messages - randomly selected
                    //  - also implement a "/help" command type
                    if (tasks.Count == 0)
                        return context.WriteText("Hey, you're cool, but I don't understand what you're trying to do.");

                    return Task.WhenAll(tasks);
                };

                token.WaitHandle.WaitOne();

                client.Disconnect();

            }, token);
        }

        public Task Worker { get; }
    }
}