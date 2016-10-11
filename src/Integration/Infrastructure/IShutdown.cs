using System.Threading;

namespace Vertica.Integration.Infrastructure
{
    public interface IShutdown
    {
        /// <summary>
        /// Provides access to the CancellationToken which can be passed to any async operation, or it can be used to be notified once we're shutting down.
        /// </summary>
        CancellationToken Token { get; }

        /// <summary>
        /// Blocks until shutdown is signaled (shutdown logic depends on the active implementation of <see cref="IWaitForShutdownRequest"/>).
        /// </summary>
        void WaitForShutdown();
    }
}