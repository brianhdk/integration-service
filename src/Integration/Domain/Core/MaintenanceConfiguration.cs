using System;

namespace Vertica.Integration.Domain.Core
{
    public class MaintenanceConfiguration
    {
        public MaintenanceConfiguration()
        {
            CleanUpTaskLogEntriesOlderThan = TimeSpan.FromDays(60);
            CleanUpErrorLogEntriesOlderThan = TimeSpan.FromDays(60);
            CleanUpArchivesOlderThan = TimeSpan.FromDays(60);
        }

        public TimeSpan CleanUpTaskLogEntriesOlderThan { get; set; }
        public TimeSpan CleanUpErrorLogEntriesOlderThan { get; set; }
        public TimeSpan CleanUpArchivesOlderThan { get; set; }
    }
}