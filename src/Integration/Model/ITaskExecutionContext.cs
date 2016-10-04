namespace Vertica.Integration.Model
{
    public interface ITaskExecutionContext
    {
        ILog Log { get; }
        Arguments Arguments { get; }

        //void ThrowIfCancelled();
        //CancellationToken CancellationToken { get; }
    }
}