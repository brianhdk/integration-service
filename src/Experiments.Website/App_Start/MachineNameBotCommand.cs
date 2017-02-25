using System;
using System.Threading;
using System.Threading.Tasks;
using Vertica.Integration.Slack.Bot.Commands;

namespace Experiments.Website
{
    public class MachineNameBotCommand : ISlackBotCommand
    {
        public bool TryHandle(SlackBotCommandContext context, CancellationToken token, out Task task)
        {
            task = null;

            if (!string.Equals(context.IncomingMessage.Text, "Hi", StringComparison.OrdinalIgnoreCase))
                return false;

            task = context.WriteText($"I'm : {Environment.MachineName}.");

            return true;
        }
    }
}