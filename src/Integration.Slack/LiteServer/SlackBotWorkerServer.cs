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

        public Task Create(CancellationToken token, BackgroundServerContext context)
        {
            return _bot.Worker;
        }
	}
}