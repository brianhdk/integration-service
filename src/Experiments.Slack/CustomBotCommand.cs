using System.Threading;
using System.Threading.Tasks;
using Vertica.Integration.Slack.Bot.Commands;
using Vertica.Integration.Slack.Messaging;
using Vertica.Integration.Slack.Messaging.Messages;

namespace Experiments.Slack
{
    public class CustomBotCommand : ISlackBotCommand
    {
        private readonly ISlackMessageQueue _messageQueue;

        public CustomBotCommand(ISlackMessageQueue messageQueue)
        {
            _messageQueue = messageQueue;
        }

        public bool TryHandle(SlackBotCommandContext context, CancellationToken token, out Task task)
        {
            task = null;

            if (context.IncomingMessage.Text.Contains("CustomBot"))
            {
                task = context.WriteText("Hello bot")
                    .ContinueWith(t => _messageQueue.Add(new SlackPostMessageInChannel("Message in channel")), token)
                    .ContinueWith(t => context.WriteText("Some other text"), token);
            }

            return task != null;
        }
    }
}