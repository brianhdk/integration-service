using System.Linq;
using Castle.DynamicProxy;
using Vertica.Integration.Slack.Messaging;
using Vertica.Integration.Slack.Messaging.Messages;

namespace Vertica.Integration.Slack
{
    internal class SlackConsoleWriterInterceptor : IInterceptor
    {
        private readonly ISlackMessageQueue _queue;

        public SlackConsoleWriterInterceptor(ISlackMessageQueue queue)
        {
            _queue = queue;
        }

        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();

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