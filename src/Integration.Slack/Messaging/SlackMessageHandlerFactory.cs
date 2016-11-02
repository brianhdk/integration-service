using Castle.MicroKernel;
using Vertica.Integration.Slack.Messaging.Handlers;

namespace Vertica.Integration.Slack.Messaging
{
    public class SlackMessageHandlerFactory : ISlackMessageHandlerFactory
    {
        private readonly IKernel _kernel;

        public SlackMessageHandlerFactory(IKernel kernel)
        {
            _kernel = kernel;
        }

        public IHandleMessages<T> Get<T>() where T : ISlackMessage
        {
            try
            {
                return _kernel.Resolve<IHandleMessages<T>>();
            }
            catch (ComponentNotFoundException ex)
            {
                throw new MessageHandlerNotFoundException($@"Unable to resolve a class that can handle {nameof(IHandleMessages<T>)}. 
Did you remember to register the handler?

See example below on how to register custom handlers:

.UseSlack(slack => slack
    .MessageHandlers(messageHandlers => messageHandlers
        .Add<MyCustomMessageHandler>())", ex);
            }
        }
    }
}