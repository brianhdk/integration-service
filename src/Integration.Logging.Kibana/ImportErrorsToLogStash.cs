using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace Vertica.Integration.Logging.Kibana
{
    public class ImportErrorsToLogStash : Step<MonitorWorkItem>
    {
        public override void Execute(MonitorWorkItem workItem, Log log)
        {
            foreach (MonitorEntry entries in workItem.GetEntries(Target.All))
            {
                // kode til at smække fejl i enten elastic search, logstash eller noget andet...
            }
        }

        public override string Description
        {
            get { return "Imports errors to Kibana through LogStash."; }
        }
    }
}