using System;
using System.Threading;
using System.Threading.Tasks;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Slack.Bot.Commands
{
    internal class PingCommand : ISlackBotCommand
    {
        private readonly IUptime _uptime;

        public PingCommand(IUptime uptime)
        {
            _uptime = uptime;
        }

        public bool TryHandle(SlackBotCommandContext context, CancellationToken token, out Task task)
        {
            task = null;

            if (!string.Equals(context.IncomingMessage.Text, "Ping", StringComparison.OrdinalIgnoreCase))
                return false;

            task = context.WriteText($"We're up and running, thanks. Uptime: {_uptime.UptimeText}.");

            return true;
        }
    }
}