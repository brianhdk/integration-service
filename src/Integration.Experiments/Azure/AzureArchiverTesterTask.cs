using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments.Azure
{
    public class AzureArchiverTesterTask : Task
    {
        private readonly IArchiver _archiver;

        public AzureArchiverTesterTask(IArchiver archiver)
        {
            _archiver = archiver;
        }

        public override void StartTask(Log log, params string[] arguments)
        {
            log.Warning("{0}", _archiver.ArchiveText("Some name", "Some content"));

            //_archiver.Delete(DateTimeOffset.UtcNow.AddHours(-2));
        }

        public override string Description
        {
            get { return "TBD"; }
        }
    }
}