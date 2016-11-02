using System.Threading;
using System.Threading.Tasks;
using Vertica.Integration.Domain.LiteServer;
using Vertica.Integration.Slack.Messaging;

namespace Vertica.Integration.Slack.LiteServer
{
    internal class SlackMessageQueueConsumerServer : IBackgroundServer
    {
        private readonly ISlackMessageQueue _queue;

        public SlackMessageQueueConsumerServer(ISlackMessageQueue queue)
        {
            _queue = queue;
        }

        public Task Create(CancellationToken token, BackgroundServerContext context)
        {
            return _queue.Consumer;
        }
    }
}