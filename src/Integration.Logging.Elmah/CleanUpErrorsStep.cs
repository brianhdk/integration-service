using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Model;
using Vertica.Utilities_v4.Extensions.StringExt;

namespace Vertica.Integration.Logging.Elmah
{
    public class CleanUpErrorsStep : Step<MaintenanceWorkItem>
    {
        private readonly string _connectionStringName;
        private readonly TimeSpan _olderThan;

        public CleanUpErrorsStep(string connectionStringName, TimeSpan olderThan)
        {
            _connectionStringName = connectionStringName;
            _olderThan = olderThan;
        }

        public override void Execute(MaintenanceWorkItem workItem, Log log)
        {
            string connectionString = ConfigurationManager.ConnectionStrings[_connectionStringName].ConnectionString;

            DateTime lowerBound = DateTime.UtcNow.Date.Subtract(_olderThan);

            using (var connection = new SqlConnection(connectionString))
            using (var command = connection.CreateCommand())
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