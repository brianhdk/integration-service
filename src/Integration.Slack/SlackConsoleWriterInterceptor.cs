using System.Linq;
using Castle.DynamicProxy;
using Vertica.Integration.Slack.Messaging;
using Vertica.Integration.Slack.Messaging.Messages;

namespace Vertica.Integration.Slack
{
    internal class SlackConsoleWriterInterceptor : IInterceptor
    {
        private readonly ISlackMessageQueue _queue;
        private readonly ISlackConfiguration _configuration;

        public SlackConsoleWriterInterceptor(ISlackMessageQueue queue, ISlackConfiguration configuration)
        {
            _queue = queue;
            _configuration = configuration;
        }

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

            if (_configuration.Enabled)
            {
                var message = (string)invocation.Arguments.FirstOrDefault();

                if (message != null)
                {
                    var args = (object[])invocation.Arguments.ElementAtOrDefault(1);

                    if (args != null)
                        message = string.Format(message, args);

                    _queue.Add(new SlackPostMessageInChannel(message));
                }
            }
        }
    }
}