using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Model;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.Logging.Elmah
{
    public class CleanUpElmahErrorsStep : Step<MaintenanceWorkItem>
    {
        private readonly string _connectionStringName;
        private readonly TimeSpan _olderThan;

        public CleanUpElmahErrorsStep(string connectionStringName, TimeSpan olderThan)
        {
            if (String.IsNullOrWhiteSpace(connectionStringName)) throw new ArgumentException(@"Value cannot be null or empty.", "connectionStringName");

            _connectionStringName = connectionStringName;
            _olderThan = olderThan;
        }

        public override void Execute(MaintenanceWorkItem workItem, Log log)
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[_connectionStringName];

            if (settings == null)
                throw new InvalidOperationException(
                    String.Format("Found no ConnectionString with name '{0}'. Please add this to the <connectionString> element.", _connectionStringName));

            DateTime lowerBound = DateTime.UtcNow.Date.Subtract(_olderThan);

            using (var connection = new SqlConnection(settings.ConnectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                command.CommandType = CommandType.Text;
                command.CommandText = "DELETE FROM [ELMAH_Error] WHERE [TimeUtc] <= @t";
                command.Parameters.AddWithValue("t", lowerBound);

                int count = command.ExecuteNonQuery();

                if (count > 0)
                    log.Message("Deleted {0} entries older than '{1}'.", count, lowerBound);
            }
        }

        public override string Description
        {
            get
            {
                return "Deletes Elmah entries older than {0} days".FormatWith(_olderThan.TotalDays);
            }
        }
    }
}