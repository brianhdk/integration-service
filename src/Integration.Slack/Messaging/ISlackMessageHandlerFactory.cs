using Vertica.Integration.Slack.Messaging.Handlers;

namespace Vertica.Integration.Slack.Messaging
{
    public interface ISlackMessageHandlerFactory
    {
        IHandleMessages<T> Get<T>() where T : ISlackMessage;
    }
}