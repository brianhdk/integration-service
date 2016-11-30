using System;
using System.Data.SqlClient;
using System.Linq;
using System.Xml;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Model;

namespace Vertica.Integration.Logging.Elmah
{
    public class ExportElmahErrorsStep : Step<MonitorWorkItem>
    {
        private const string ConfigurationName = "24E20065-43F9-42DB-90AA-09823637C00C";
        internal const string MessageGroupingPattern = @"Details:\ /elmah\.axd/detail\?id=.+$";

        private readonly IConfigurationService _configuration;

        public ExportElmahErrorsStep(IConfigurationService configuration)
        {
            _configuration = configuration;
        }

        public override Execution ContinueWith(MonitorWorkItem workItem)
        {
            ElmahConfiguration configuration = _configuration.GetElmahConfiguration();

            if (configuration.GetConnectionString() == null)
                return Execution.StepOver;

            if (configuration.Disabled)
                return Execution.StepOver;

            workItem.Context(ConfigurationName, configuration);

            return Execution.Execute;
        }

        public override void Execute(MonitorWorkItem workItem, ITaskExecutionContext context)
        {
            ElmahConfiguration configuration = workItem.Context<ElmahConfiguration>(ConfigurationName);

            workItem.AddMessageGroupingPatterns(MessageGroupingPattern);

            using (var connection = new SqlConnection(configuration.GetConnectionString()))
            using (SqlCommand command = connection.CreateCommand())
            {
                connection.Open();

				command.CommandTimeout = (int)configuration.CommandTimeout.TotalSeconds;

                command.CommandText = @"
UPDATE [ELMAH_Error] SET 
    [AllXml] = REPLACE(CAST([AllXml] AS NVARCHAR(MAX)), '&#x', '')
WHERE [TimeUtc] BETWEEN @l AND @u;

SELECT TOP 1000
	errorId     = [ErrorId], 
	application = [Application],
    host        = [Host], 
    type        = [Type],
    source      = [Source],
    message     = [Message],
    [user]      = [User],
    statusCode  = [StatusCode], 
    time        = CONVERT(VARCHAR(50), [TimeUtc], 126) + 'Z',
	CAST([AllXml] AS XML).value('(/error/serverVariables/item[@name=''URL'']/value/@string)[1]', 'nvarchar(max)') AS url,
	CAST([AllXml] AS XML).value('(/error/serverVariables/item[@name=''QUERY_STRING'']/value/@string)[1]', 'nvarchar(max)') AS query_string,
	CAST([AllXml] AS XML).value('(/error/serverVariables/item[@name=''HTTP_REFERER'']/value/@string)[1]', 'nvarchar(max)') AS http_referer
FROM [ELMAH_Error] error 
WHERE [TimeUtc] BETWEEN @l AND @u
ORDER BY [TimeUtc] DESC, [Sequence] DESC
FOR XML AUTO";

                command.Parameters.AddWithValue("l", workItem.CheckRange.LowerBound);
                command.Parameters.AddWithValue("u", workItem.CheckRange.UpperBound);

                using (XmlReader reader = command.ExecuteXmlReader())
                {
                    try
                    {
                        int count = 0;

                        while (reader.IsStartElement("error"))
                        {
                            count++;
                            var error = new ElmahError(reader);

                            workItem.Add(
                                error.Created,
								string.Join(", ", new[] {configuration.LogName, error.Source}
                                    .Where(x => !string.IsNullOrWhiteSpace(x))),
                                error.ToString());
                        }

                        if (count > 0)
                        {
                            context.Log.Message("{0} entries within time-period {1}.", count, workItem.CheckRange);
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new InvalidOperationException(
	                        $@"{ex.Message}

---
You need to locate the row causing this error. The easiest way is to query those rows that were part of the initial selection. 
Open the XML result in SQL Server Management Studio, and find the one(s) with a red squiggles. They need to be removed.
Find out where errors like that are being generated and make sure to encode these messages, so that it won't happen again.

CommandText was: 

{command
		                        .CommandText}", ex);                        
                    }
                    catch (XmlException ex)
                    {
                        throw new InvalidOperationException(
	                        $@"Unable to parse XML. Error:

{ex.Message}

---
You need to locate the row causing this error. The easiest way is to query those rows that were part of the initial selection. 
Open the XML result in SQL Server Management Studio, and find the one(s) with a red squiggles. They need to be removed.
Find out where errors like that are being generated and make sure to encode these messages, so that it won't happen again.

CommandText was: 

{command
		                        .CommandText}", ex);
                    }
                }
            }
        }

        public override string Description => "Exports errors from Elmah log.";
    }
}