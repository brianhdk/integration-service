using System;
using Vertica.Integration.Domain.Monitoring;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Model;

namespace Vertica.Integration.Experiments
{
    public class XssTestingStep : Step<MonitorWorkItem>
    {
        public override void Execute(MonitorWorkItem workItem, ILog log)
        {
            workItem.Add(new MonitorEntry(DateTimeOffset.Now, "Source", "<b>Some message</b>"), Target.Service);
        }

        public override string Description
        {
            get { return "TBD"; }
        }
    }
}