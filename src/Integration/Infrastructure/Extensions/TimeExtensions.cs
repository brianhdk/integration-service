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

            IEnumerable<string> Segments()
            {
                if (span.Days > 0)
                    yield return $"{span.Days} day{(span.Days == 1 ? string.Empty : "s")}";

                if (span.Hours > 0)
                    yield return $"{span.Hours} hour{(span.Hours == 1 ? string.Empty : "s")}";

                if (span.Minutes > 0)
                    yield return $"{span.Minutes} minute{(span.Minutes == 1 ? string.Empty : "s")}";

                if (span.Seconds > 0)
                    yield return $"{span.Seconds} second{(span.Seconds == 1 ? string.Empty : "s")}";
            }

            return string.Join(" ", Segments());
        }
    }
}