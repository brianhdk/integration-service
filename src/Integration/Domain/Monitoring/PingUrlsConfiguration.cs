using System;

namespace Vertica.Integration.Domain.Monitoring
{
    public class PingUrlsConfiguration
    {
        public PingUrlsConfiguration()
        {
            MaximumWaitTimeSeconds = (uint) TimeSpan.FromMinutes(2).TotalSeconds;
        }

        public uint MaximumWaitTimeSeconds { get; set; }

        public string[] Urls { get; set; }
    }
}