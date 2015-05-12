using System;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Logging.Elmah
{
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
            if (String.IsNullOrWhiteSpace(ConnectionStringName))
                return null;

            return ConnectionString.FromName(ConnectionStringName);
        }
    }
}