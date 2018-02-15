using System;

namespace Vertica.Integration.Infrastructure
{
    public interface IUptime
    {
        DateTimeOffset StartedAt { get; }

        string UptimeText { get; }
    }
}