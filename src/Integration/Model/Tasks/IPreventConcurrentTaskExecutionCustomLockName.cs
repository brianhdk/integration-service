namespace Vertica.Integration.Model.Tasks
{
    /// <summary>
    /// Implement this interface to decide at runtime what Name to use on a Distributed Lock for Tasks marked with <see cref="PreventConcurrentTaskExecutionAttribute"/>.
    /// </summary>
    public interface IPreventConcurrentTaskExecutionCustomLockName
    {
        string GetLockName(ITask currentTask, Arguments arguments);
    }
}