using System;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class ArchiveTesterTask : Task
    {
        private readonly IArchiveService _archiver;
        private readonly ITaskFactory _taskFactory;
        private readonly ITaskRunner _taskRunner;

        public ArchiveTesterTask(IArchiveService archiver, ITaskFactory taskFactory, ITaskRunner taskRunner)
        {
            _archiver = archiver;
            _taskFactory = taskFactory;
            _taskRunner = taskRunner;
        }

        public override string Description
        {
            get { return "TBD"; }
        }

        public override void StartTask(ITaskExecutionContext context)
        {
            context.Log.Message(_archiver.ArchiveText("Import", "Some content", options => options.GroupedBy("My Group").ExpiresAfterDays(1)));

            //_taskRunner.Execute(_taskFactory.Get<MaintenanceTask>());
        }
    }
}