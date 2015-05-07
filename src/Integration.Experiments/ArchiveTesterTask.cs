using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class ArchiveTesterTask : Task
    {
        private readonly IArchiveService _archive;

        public ArchiveTesterTask(IArchiveService archive)
        {
            _archive = archive;
        }

        public override void StartTask(ILog log, params string[] arguments)
        {
            log.Warning("{0}", _archive.ArchiveText("Some name", "Some content"));

            //_archive.Delete(DateTimeOffset.UtcNow.AddHours(-2));
        }

        public override string Description
        {
            get { return "TBD"; }
        }
    }
}