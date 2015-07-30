namespace Vertica.Integration.Model
{
	public interface ITaskRunner
	{
        TaskExecutionResult Execute(ITask task, Arguments arguments = null);
	}
}