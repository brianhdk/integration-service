using System;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Model;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public class CleanUpArchivesStep : Step<MaintenanceWorkItem>
    {
        private readonly IArchiveService _archive;
        private readonly TimeSpan _olderThan;

        public CleanUpArchivesStep(IArchiveService archive, TimeSpan olderThan)
        {
            _archive = archive;
            _olderThan = olderThan;
        }

        public override void Execute(MaintenanceWorkItem workItem, Log log)
        {
            DateTimeOffset lowerBound = Time.UtcNow.Subtract(_olderThan);

            int count = _archive.Delete(lowerBound);

            if (count > 0)
                log.Message("Deleted {0} archives older than '{1}'.", count, lowerBound);
        }

        public override string Description
        {
            get
            {
                return "Deletes archives older than {0} days".FormatWith(_olderThan.TotalDays);
            }
        }
    }
}