namespace Vertica.Integration.Model
{
    public interface ITaskExecutionContext
    {
        ILog Log { get; }
        string[] Arguments { get; }
    }
}