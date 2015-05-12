using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Vertica.Integration.Domain.Core
{
    [Guid("FBF783F5-0210-448D-BEB9-FD0E9AD6CABF")]
    [Description("Used by the MaintenanceTask.")]
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