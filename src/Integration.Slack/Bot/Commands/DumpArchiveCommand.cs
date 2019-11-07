using System.Threading;
using System.Threading.Tasks;
using Vertica.Integration.Infrastructure.Archiving;

namespace Vertica.Integration.Slack.Bot.Commands
{
    internal class DumpArchiveCommand : ISlackBotCommand
    {
        private readonly IArchiveService _archiveService;

        public DumpArchiveCommand(IArchiveService archiveService)
        {
            _archiveService = archiveService;
        }

        public bool TryHandle(SlackBotCommandContext context, CancellationToken token, out Task task)
        {
            // dumper et arkiv og poster til en channel (som attachment)

            task = null;
            return false;
        }
    }
}