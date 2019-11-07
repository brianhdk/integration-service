namespace Vertica.Integration.Model.Tasks
{
    /// <summary>
    /// Implement this interface to decide at runtime what Description to use on a Distributed Lock for Tasks marked with <see cref="PreventConcurrentTaskExecutionAttribute"/>.
    /// </summary>
    public interface IPreventConcurrentTaskExecutionCustomLockDescription
    {
        string GetLockDescription(ITask currentTask, Arguments arguments, string currentDescription);
    }
}