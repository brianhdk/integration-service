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
            CleanUpEntriesOlderThan = TimeSpan.FromDays(60);
        }

        public string LogName { get; set; }
        public string ConnectionStringName { get; set; }
        public TimeSpan CleanUpEntriesOlderThan { get; set; }

        public ConnectionString ToConnectionString()
        {
            if (string.IsNullOrWhiteSpace(ConnectionStringName))
                return null;

            return ConnectionString.FromName(ConnectionStringName);
        }
    }
}