using System;
using System.Threading;
using System.Threading.Tasks;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Slack.Bot.Commands
{
    internal class PingCommand : ISlackBotCommand
    {
        private readonly IUptime _uptime;
        private readonly string _postFix;

        public PingCommand(IUptime uptime, IRuntimeSettings settings)
        {
            _uptime = uptime;
            _postFix = settings.Environment;

            if (!string.IsNullOrWhiteSpace(_postFix))
                _postFix = $" ({_postFix})";
        }

        public bool TryHandle(SlackBotCommandContext context, CancellationToken token, out Task task)
        {
            task = null;

            if (!string.Equals(context.IncomingMessage.Text, "Ping", StringComparison.OrdinalIgnoreCase))
                return false;



            task = context.WriteText($"[{Environment.MachineName}{_postFix}]: I'm up and running, thanks. Uptime: {_uptime.UptimeText}.");

            return true;
        }
    }
}