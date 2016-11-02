using System.Threading;
using System.Threading.Tasks;
using SlackConnector;
using SlackConnector.Models;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Slack.Bot
{
    public class SlackBot : ISlackBot
    {
        private readonly CancellationToken _token;

        public SlackBot(ISlackConfiguration configuration, IShutdown shutdown)
        {
            if (!configuration.Enabled)
                return;

            _token = shutdown.Token;

            Worker = Task.Run(() =>
            {
                SlackConnector.SlackConnector connector = new SlackConnector.SlackConnector();

                ISlackConnection client = connector.Connect(configuration.BotUserToken).Result;

                client.OnMessageReceived += message =>
                {
                    var response = new BotMessage
                    {
                        Text = message.Text,
                        ChatHub = message.ChatHub
                    };

                    Thread.Sleep(1000);

                    return client.Say(response);
                };

                _token.WaitHandle.WaitOne();

                client.Disconnect();

            }, _token);
        }

        public Task Worker { get; }
    }
}