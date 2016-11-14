using System.Threading;
using System.Threading.Tasks;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Slack.Bot;

namespace Vertica.Integration.Slack.LiteServer
{
    internal class SlackBotWorkerServer : IBackgroundServer
    {
        private readonly ISlackBot _bot;

        public SlackBotWorkerServer(ISlackBot bot)
        {
            _bot = bot;
        }

        public Task Create(BackgroundServerContext context, CancellationToken token)
        {
            return _bot.Worker;
        }
	}
}