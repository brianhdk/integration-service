using System;
using System.Data;
using System.Data.SqlClient;
using Vertica.Integration.Domain.Core;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Model;

namespace Vertica.Integration.Logging.Elmah
{
    public class CleanUpElmahErrorsStep : Step<MaintenanceWorkItem>
    {
        private readonly IConfigurationService _configuration;

        public CleanUpElmahErrorsStep(IConfigurationService configuration)
        {
            _configuration = configuration;
        }

        public override Execution ContinueWith(MaintenanceWorkItem workItem)
        {
            ElmahConfiguration configuration = _configuration.GetElmahConfiguration();

            if (String.IsNullOrWhiteSpace(configuration.ConnectionStringName))
                return Execution.StepOver;

            return Execution.Execute;
        }

        public override void Execute(MaintenanceWorkItem workItem, ILog log)
        {
            ElmahConfiguration configuration = _configuration.GetElmahConfiguration();

            DateTime lowerBound = DateTime.UtcNow.Date.Subtract(configuration.CleanUpEntriesOlderThan);

            using (var connection = new SqlConnection(configuration.ToConnectionString()))
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
                return String.Format("Deletes Elmah entries older than {0} days",
                    _configuration.GetElmahConfiguration().CleanUpEntriesOlderThan.TotalDays);
            }
        }
    }
}