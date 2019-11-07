using System;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Utilities;

namespace Vertica.Integration.Infrastructure
{
    public class UptimeTextGenerator : IUptimeTextGenerator
    {
        public string GetUptimeText(DateTimeOffset startTime)
        {
            TimeSpan span = Time.UtcNow - startTime;

            return span.ToPrettyDuration();
        }
    }
}