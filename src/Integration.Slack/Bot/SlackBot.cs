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

        public SlackBot(IShutdown shutdown)
        {
            _token = shutdown.Token;

            Worker = Task.Run(() =>
            {
                SlackConnector.SlackConnector connector = new SlackConnector.SlackConnector();

                ISlackConnection client = connector.Connect("xoxb-63343022768-V6GIejWL2hIgM3dWkxt7tYS5").Result;

                client.OnMessageReceived += message =>
                {
                    //_writer.WriteLine(message.Text);

                    var response = new BotMessage
                    {
                        Text = message.Text,
                        ChatHub = message.ChatHub
                    };

                    return client.Say(response);
                };

                _token.WaitHandle.WaitOne();

                client.Disconnect();

            }, _token);
        }

        public Task Worker { get; }
    }
}