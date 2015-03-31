using System;
using System.Linq;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Domain.Monitoring
{
    public class MonitorConfiguration
    {
        public MonitorConfiguration()
        {
            Targets = new[]
            {
                new MonitorTarget(Target.Service)
            };

            SubjectPrefix = "Integration Service";
        }

        public DateTimeOffset LastRun { get; set; }

        public string[] IgnoreErrorsWithMessagesContaining { get; set; }
        public MonitorTarget[] Targets { get; set; }
        public string SubjectPrefix { get; set; }

        public void Assert()
        {
            if (Targets == null)
                throw new InvalidOperationException("No targets defined for MonitorConfiguration.");

            MonitorTarget service = Targets.SingleOrDefault(x => x.Equals(Target.Service));

            if (service == null)
                throw new InvalidOperationException(
                    String.Format("Missing required target '{0}' for MonitorConfiguration.", Target.Service));
        }
    }
}