using Hangfire.Server;

namespace Vertica.Integration.Hangfire.Console
{
    internal class HangfirePerThreadPerformContext
    {
        public PerformContext Value { get; set; }
    }
}