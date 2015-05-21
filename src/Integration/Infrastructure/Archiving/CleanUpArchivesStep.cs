using System;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Model;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Extensions.TimeExt;

namespace Vertica.Integration.Infrastructure.Archiving
{
    public class CleanUpArchivesStep : Step<MaintenanceWorkItem>
    {
        private readonly IArchiveService _archive;
        private readonly IConfigurationService _configuration;

        public CleanUpArchivesStep(IArchiveService archive, IConfigurationService configuration)
        {
            _archive = archive;
            _configuration = configuration;
        }

        public override void Execute(MaintenanceWorkItem workItem, ITaskExecutionContext context)
        {
            DateTimeOffset lowerBound = Time.UtcNow.BeginningOfDay().Subtract(workItem.Configuration.CleanUpArchivesOlderThan);

            int count = _archive.Delete(lowerBound);

            if (count > 0)
                context.Log.Message("Deleted {0} archives older than '{1}'.", count, lowerBound);
        }

        public override string Description
        {
            get
            {
                MaintenanceConfiguration configuration = _configuration.Get<MaintenanceConfiguration>();

                return String.Format("Deletes archives older than {0} days.",
                    configuration.CleanUpArchivesOlderThan.TotalDays);
            }
        }
    }
}