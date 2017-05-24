using Hangfire.Server;

namespace Vertica.Integration.Hangfire.Console
{
    public interface IHangfirePerformContextProvider
    {
        /// <summary>
        /// This exposes the PerformContext instance, which is set (on the actual thread) before Hangfire executes a job.
        /// </summary>
        PerformContext Current { get; }
    }
}