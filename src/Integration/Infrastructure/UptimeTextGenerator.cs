using System;
using System.Collections.Generic;
using Vertica.Utilities;

namespace Vertica.Integration.Infrastructure
{
    public class UptimeTextGenerator : IUptimeTextGenerator
    {
        public string GetUptimeText(DateTimeOffset startTime)
        {
            TimeSpan span = Time.UtcNow - startTime;

            if (span.TotalSeconds < 1)
                return $"{span.TotalSeconds} seconds";

            var segments = new List<string>(4);

            if (span.Days > 0)
                segments.Add($"{span.Days} day{(span.Days == 1 ? string.Empty : "s")}");

            if (span.Hours > 0)
                segments.Add($"{span.Hours} hour{(span.Hours == 1 ? string.Empty : "s")}");

            if (span.Minutes > 0)
                segments.Add($"{span.Minutes} minute{(span.Minutes == 1 ? string.Empty : "s")}");

            if (span.Seconds > 0)
                segments.Add($"{span.Seconds} second{(span.Seconds == 1 ? string.Empty : "s")}");

            segments.Add($"(started at {startTime} (UTC))");

            return string.Join(" ", segments);

        }
    }
}