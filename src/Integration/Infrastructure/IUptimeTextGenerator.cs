using System;

namespace Vertica.Integration.Infrastructure
{
    public interface IUptimeTextGenerator
    {
        string GetUptimeText(DateTimeOffset startTime);
    }
}