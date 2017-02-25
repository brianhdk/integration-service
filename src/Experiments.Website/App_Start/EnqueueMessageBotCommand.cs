using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Vertica.Integration.Slack.Bot.Commands;

namespace Experiments.Website
{
    public class EnqueueMessageBotCommand : ISlackBotCommand
    {
        public bool TryHandle(SlackBotCommandContext context, CancellationToken token, out Task task)
        {
            task = null;

            string messageToMachine = $"{Environment.MachineName}: ";

            if (!context.IncomingMessage.Text.StartsWith(messageToMachine, StringComparison.OrdinalIgnoreCase))
                return false;

            string message = context.IncomingMessage.Text.Substring(0, messageToMachine.Length);

            if (string.IsNullOrWhiteSpace(message))
                return false;

            string id = BackgroundJob.Enqueue<IEnqueueTask>(x =>
                x.Run(nameof(WriteToSlackTask), new KeyValuePair<string, string>("message", message)));

            task = context.WriteText($"Job with ID '{id}' has been added to Hangfire on ${Environment.MachineName}.");

            return true;
        }
    }
}