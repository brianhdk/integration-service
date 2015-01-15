namespace Vertica.Integration.Model
{
	public interface ITaskRunner
	{
        TaskExecutionResult Execute(string taskName, ITask task, params string[] arguments);
	}
}