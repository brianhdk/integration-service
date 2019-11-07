using System;
using System.Threading.Tasks;
using SlackConnector.Models;

namespace Vertica.Integration.Slack.Bot.Commands
{
    public class SlackBotCommandContext
    {
        public SlackBotCommandContext(Func<BotMessage, Task> writeMessage, SlackMessage incomingMessage)
        {
            if (writeMessage == null) throw new ArgumentNullException(nameof(writeMessage));
            if (incomingMessage == null) throw new ArgumentNullException(nameof(incomingMessage));

            WriteMessage = writeMessage;
            IncomingMessage = incomingMessage;
        }

        public SlackMessage IncomingMessage { get; }

        public Func<BotMessage, Task> WriteMessage { get; }

        public Func<string, Task> WriteText => text => WriteMessage(new BotMessage { Text = text, ChatHub = IncomingMessage.ChatHub });
    }
}