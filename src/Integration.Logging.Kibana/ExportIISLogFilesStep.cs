using System;
using System.IO;
using System.Linq;
using Vertica.Integration.Model;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Logging.Kibana
{
    public class ExportIisLogFilesStep : Step<ExportLogsToLogStashWorkItem>
    {
        public override string Description => "Exports Log files from IIS";

	    public override void Execute(ExportLogsToLogStashWorkItem workItem, ITaskExecutionContext context)
        {
            // konfiguration af tidspunkt
            var within = new Range<DateTimeOffset>(DateTimeOffset.UtcNow.AddYears(-2), DateTimeOffset.UtcNow);

            foreach (var logPath in new[] { @"\\pj-web01\c$\inetpub\logs\LogFiles\W3SVC2" })
            {
                // understøtte flere servere - konfiguration
                var files = Directory.EnumerateFiles(logPath)
                                .Select(x => new FileInfo(x))
                                .Where(x => within.Contains(x.LastWriteTimeUtc))
                                .OrderByDescending(x => x.LastWriteTimeUtc);

                // kig på om vi kan sende den samme fil igen til LogStash (om den håndterer det pænt)
                foreach (FileInfo file in files)
                {
                    using (FileStream fileStream = file.OpenRead())
                    {
                        workItem.Upload(fileStream, file.Name);
                    }
                }                
            }
        }
    }
}