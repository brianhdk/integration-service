namespace Vertica.Integration.Model.Tasks
{
    /// <summary>
    /// Implement this interface to decide at runtime how Tasks marked with <see cref="PreventConcurrentTaskExecutionAttribute"/> should be handled in regards to concurrency.
    /// </summary>
    public interface IPreventConcurrentTaskExecutionRuntimeEvaluator
    {
        bool Disabled(ITask currentTask);
    }
}