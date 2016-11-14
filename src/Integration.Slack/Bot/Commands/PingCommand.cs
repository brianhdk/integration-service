using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vertica.Integration.Slack.Bot.Commands
{
    internal class PingCommand : ISlackBotCommand
    {
        // fortæller evt. up-time bot'en

        public bool TryHandle(SlackBotCommandContext context, CancellationToken token, out Task task)
        {
            task = null;

            if (!string.Equals(context.IncomingMessage.Text, "Ping", StringComparison.OrdinalIgnoreCase))
                return false;

            task = context.WriteText("We're up and running, thanks");

            return true;
        }
    }
}