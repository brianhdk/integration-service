using System;

namespace Vertica.Integration.Slack.Messaging.Messages
{
    public class SlackPostMessageInChannel : ISlackMessage
    {
        public SlackPostMessageInChannel(string text, string channel = null)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException(@"Value cannot be null or empty", nameof(text));
            
            Text = text;
            Channel = channel;
        }

        public string Text { get; }
        public string Channel { get; }
    }
}