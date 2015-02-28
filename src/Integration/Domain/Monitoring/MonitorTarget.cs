using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Domain.Monitoring
{
    public class MonitorTarget : Target
    {
        public MonitorTarget(string name)
            : base(name)
        {
        }

        public string[] Recipients { get; set; }
    }
}