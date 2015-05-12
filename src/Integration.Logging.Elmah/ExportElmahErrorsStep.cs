using System;
using System.Data.SqlClient;
using System.Xml;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace Vertica.Integration.Logging.Elmah
{
    public class ExportElmahErrorsStep : Step<MonitorWorkItem>
    {
        private readonly IConfigurationService _configuration;

        public ExportElmahErrorsStep(IConfigurationService configuration)
        {
            _configuration = configuration;
        }

        public override Execution ContinueWith(MonitorWorkItem workItem)
        {
            ElmahConfiguration configuration = _configuration.GetElmahConfiguration();

            if (String.IsNullOrWhiteSpace(configuration.ConnectionStringName))
                return Execution.StepOver;

            return Execution.Execute;
        }

        public override void Execute(MonitorWorkItem workItem, ILog log)
        {
            ElmahConfiguration configuration = _configuration.GetElmahConfiguration();

            using (var connection = new SqlConnection(configuration.ToConnectionString()))
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

                using (XmlReader reader = command.ExecuteXmlReader())
                {
                    while (reader.IsStartElement("error"))
                    {
                        var error = new ElmahError(reader);

                        workItem.Add(
                            error.Created,
                            configuration.LogName, 
                            error.ToString(), 
                            Target.Service);
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