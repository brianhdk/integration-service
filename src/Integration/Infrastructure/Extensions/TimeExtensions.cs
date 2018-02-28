using System;
using System.Collections.Generic;

namespace Vertica.Integration.Infrastructure.Extensions
{
    internal static class TimeExtensions
    {
        public static string ToPrettyDuration(this TimeSpan span)
        {
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

            return string.Join(" ", segments);
        }
    }
}