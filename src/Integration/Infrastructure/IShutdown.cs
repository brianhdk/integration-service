using System.Threading;

namespace Vertica.Integration.Infrastructure
{
    public interface IShutdown
    {
        /// <summary>
        /// Provides access to the CancellationToken which can be passed to any async operation.
        /// </summary>
        CancellationToken Token { get; }

        /// <summary>
        /// Blocks until shutdown is signaled (shutdown logic depends on the active implementation of <see cref="IWaitForShutdownRequest"/>).
        /// </summary>
        void WaitForShutdown();
    }
}