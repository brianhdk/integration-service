using System.Threading.Tasks;

namespace Vertica.Integration.Slack.Messaging
{
    public interface ISlackMessageQueue
    {
        void Add<T>(T message) where T : ISlackMessage;

        Task Consumer { get; }
    }
}