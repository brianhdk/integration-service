using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace Vertica.Integration.Logging.Kibana
{
    public class ImportErrorsToLogStashStep : Step<MonitorWorkItem>
    {
        public override void Execute(MonitorWorkItem workItem, ITaskExecutionContext context)
        {
            foreach (MonitorEntry entries in workItem.GetEntries(Target.All))
            {
                // kode til at smække fejl i enten elastic search, logstash eller noget andet...
            }
        }

        public override string Description => "Imports errors to Kibana through LogStash.";
    }
}