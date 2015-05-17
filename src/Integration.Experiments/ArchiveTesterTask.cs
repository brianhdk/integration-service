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

        public override void StartTask(ILog log, params string[] arguments)
        {
            log.Message(_archiver.ArchiveText("Test", "Some Text"));
            log.Warning(Target.All, "Test-All");
            log.Warning(Target.Service, "Test-Service");
            log.Error(Target.Service, "Test-Error");
        }
    }
}