using Vertica.Integration.Domain.Core;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public class CleanUpArchivesStep : Step<MaintenanceWorkItem>
    {
        private readonly IArchiveService _archive;

        public CleanUpArchivesStep(IArchiveService archive)
        {
            _archive = archive;
        }

        public override void Execute(MaintenanceWorkItem workItem, ITaskExecutionContext context)
        {
            int count = _archive.DeleteExpired();

            if (count > 0)
                context.Log.Message("Deleted {0} expired archive(s).", count);
        }

        public override string Description => "Deletes expired archives.";
    }
}