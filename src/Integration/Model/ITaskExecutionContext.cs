using System.Threading;

namespace Vertica.Integration.Model
{
    public interface ITaskExecutionContext
    {
        ILog Log { get; }
        Arguments Arguments { get; }

        CancellationToken CancellationToken { get; }
        void ThrowIfCancelled();
    }
}