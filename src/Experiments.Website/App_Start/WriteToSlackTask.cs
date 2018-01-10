using System;
using Vertica.Integration.Model;
using Vertica.Integration.Slack.Messaging;
using Vertica.Integration.Slack.Messaging.Messages;

namespace Experiments.Website
{
    public class WriteToSlackTask : IntegrationTask
    {
        private readonly ISlackMessageQueue _slackMessageQueue;

        public WriteToSlackTask(ISlackMessageQueue slackMessageQueue)
        {
            _slackMessageQueue = slackMessageQueue;
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            var message = context.Arguments["message"];

            context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(5));

            _slackMessageQueue.Add(new SlackPostMessageInChannel($"{Environment.MachineName}: I'm processing message: {message}"));
        }

        public override string Description => "Writes to Slack!";
    }
}