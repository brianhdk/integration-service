using System.Threading;
using System.Threading.Tasks;

namespace Vertica.Integration.Slack.Messaging.Handlers
{
    public interface IHandleMessages<in T> : IHandleMessages
        where T : ISlackMessage
    {
        Task Handle(T message, CancellationToken token);
    }

    public interface IHandleMessages
    {
    }
}