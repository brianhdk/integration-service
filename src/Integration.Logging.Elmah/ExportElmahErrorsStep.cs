using System;
using System.Data.SqlClient;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace Vertica.Integration.Logging.Elmah
{
    public class ExportElmahErrorsStep : Step<MonitorWorkItem>
    {
        private readonly ConnectionString _connectionString;
        private readonly string _sourceName;

        public ExportElmahErrorsStep(string connectionStringName, string sourceName)
        {
            if (String.IsNullOrWhiteSpace(sourceName)) throw new ArgumentException(@"Value cannot be null or empty.", "sourceName");

            _connectionString = ConnectionString.FromName(connectionStringName);
            _sourceName = sourceName;
        }

        public override void Execute(MonitorWorkItem workItem, Log log)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

                command.CommandText = @"
SELECT 
	errorId     = [ErrorId], 
	application = [Application],
    host        = [Host], 
    type        = [Type],
    source      = [Source],
    message     = [Message],
    [user]      = [User],
    statusCode  = [StatusCode], 
    time        = CONVERT(VARCHAR(50), [TimeUtc], 126) + 'Z'
FROM [ELMAH_Error] error 
WHERE [TimeUtc] BETWEEN @l AND @u
ORDER BY [TimeUtc] DESC, [Sequence] DESC
FOR XML AUTO";

                command.Parameters.AddWithValue("l", workItem.CheckRange.LowerBound);
                command.Parameters.AddWithValue("u", workItem.CheckRange.UpperBound);

                using (var xmlReader = command.ExecuteXmlReader())
                {
                    while (xmlReader.IsStartElement("error"))
                    {
                        var error = new ElmahError(xmlReader);

                        workItem.Add(error.Created, _sourceName, error.ToString(), Target.Service);
                    }
                }
            }
        }

        public override string Description
        {
            get { return "Exports errors from Elmah log."; }
        }
    }
}