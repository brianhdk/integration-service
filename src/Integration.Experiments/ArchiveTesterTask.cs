using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class ArchiveTesterTask : Task
    {
        private readonly IArchiveService _archiver;

        public ArchiveTesterTask(IArchiveService archiver)
        {
            _archiver = archiver;
        }

        public override string Description
        {
            get { return "TBD"; }
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            context.Log.Message(_archiver.ArchiveText("Test", "Some Text"));
            context.Log.Warning(Target.All, "Test-All");
            context.Log.Warning(Target.Service, "Test-Service");
            context.Log.Error(Target.Service, "Test-Error");
        }
    }
}