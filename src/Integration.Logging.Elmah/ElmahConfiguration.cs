using System;
using System.Runtime.InteropServices;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Logging.Elmah
{
    [Guid("CA1BDD98-C619-490D-A741-C79B238A9891")]
    public class ElmahConfiguration
    {
        public ElmahConfiguration()
        {
            LogName = "Elmah";
            ConnectionStringName = "Logging.ElmahDb";
            ConnectionStringText = string.Empty;

            CommandTimeout = TimeSpan.FromMinutes(5);
            CleanUpEntriesOlderThan = TimeSpan.FromDays(60);
            BatchSize = 20;
        }

        public string LogName { get; set; }

        public string ConnectionStringName { get; set; }
        public string ConnectionStringText { get; set; }

        public TimeSpan CommandTimeout { get; set; }
        public TimeSpan CleanUpEntriesOlderThan { get; set; }
        public int BatchSize { get; set; }
        
        public bool Disabled { get; set; }

        public ConnectionString GetConnectionString()
        {
            if (!string.IsNullOrWhiteSpace(ConnectionStringName))
                return ConnectionString.FromName(ConnectionStringName);

            if (!string.IsNullOrWhiteSpace(ConnectionStringText))
                return ConnectionString.FromText(ConnectionStringText);

            return null;
        }
    }
}