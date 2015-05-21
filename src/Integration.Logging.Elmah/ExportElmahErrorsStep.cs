﻿using System;
using System.Data.SqlClient;
using System.Linq;
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

        public override void Execute(MonitorWorkItem workItem, ITaskExecutionContext context)
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
                    try
                    {
                        while (reader.IsStartElement("error"))
                        {
                            var error = new ElmahError(reader);

                            workItem.Add(
                                error.Created,
                                String.Join(",", new[] { configuration.LogName, error.Source }.Where(x => !String.IsNullOrWhiteSpace(x))),
                                error.ToString(),
                                Target.Service);
                        }
                    }
                    catch (XmlException ex)
                    {
                        throw new InvalidOperationException(String.Format(@"Unable to parse XML. Error:

{0}

---
You need to locate the row causing this error. The easiest way is to query those rows that were part of the initial selection. 
Open the XML result in SQL Server Management Studio, and find the one(s) with a red squiggles. They need to be removed.
Find out where errors like that are being generated and make sure to encode these messages, so that it won't happen again.

CommandText was: 

{1}", ex.Message, command.CommandText), ex);
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