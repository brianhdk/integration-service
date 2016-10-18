using System.Threading;
using System.Threading.Tasks;
using SlackConnector;
using SlackConnector.Models;
using Vertica.Integration.Domain.LiteServer;

namespace Examples.LiteServer.Servers
{
    public class SlackBotServer : IBackgroundServer
    {
        public Task Create(CancellationToken token, BackgroundServerContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    SlackConnector.SlackConnector c = new SlackConnector.SlackConnector();
                    ISlackConnection client = c.Connect("xoxb-63343022768-V6GIejWL2hIgM3dWkxt7tYS5").Result;

                    client.OnMessageReceived += message =>
                    {
                        var response = new BotMessage
                        {
                            Text = $"Right back at you: {message.Text}",
                            ChatHub = message.ChatHub
                        };

                        return client.Say(response);
                    };

                    token.WaitHandle.WaitOne();

                    client.Disconnect();
                }
            }, token);
        }
    }
}