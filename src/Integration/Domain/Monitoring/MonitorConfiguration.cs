using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Domain.Monitoring
{
    [Guid("9FF492BF-D4B5-4E67-AF72-C02EA8671051")]
    [Description("Used by the MonitorTask. Remember to set one or more recipients for each of the defined Targets.")]
    public class MonitorConfiguration
    {
        public MonitorConfiguration()
        {
            Targets = new[]
            {
                new MonitorTarget(Target.Service)
            };

            SubjectPrefix = "Integration Service";

            PingUrls = new PingUrlsConfiguration();
        }

        public DateTimeOffset LastRun { get; set; }

        public string[] IgnoreErrorsWithMessagesContaining { get; set; }
        public MonitorTarget[] Targets { get; set; }
        public string SubjectPrefix { get; set; }

        public PingUrlsConfiguration PingUrls { get; private set; }

        public void Assert()
        {
            if (Targets == null)
                throw new InvalidOperationException("No targets defined for MonitorConfiguration.");

            MonitorTarget service = Targets.SingleOrDefault(x => x.Equals(Target.Service));

            if (service == null)
                throw new InvalidOperationException(
                    String.Format("Missing required target '{0}' for MonitorConfiguration.", Target.Service));
        }

        public class PingUrlsConfiguration
        {
            public PingUrlsConfiguration()
            {
                MaximumWaitTimeSeconds = (uint)TimeSpan.FromMinutes(2).TotalSeconds;
            }

            public uint MaximumWaitTimeSeconds { get; set; }
            public string[] Urls { get; set; }
        }
    }
}