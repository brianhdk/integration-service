using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace Vertica.Integration.Slack.Bot.Commands
{
    public interface ISlackBotCommand
    {
        bool TryHandle(SlackBotCommandContext context, CancellationToken token, out Task task);
    }
}