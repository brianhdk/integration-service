namespace Vertica.Integration.Model
{
	public interface ITaskFactory
	{
	    bool Exists(string name);

	    ITask Get<TTask>() where TTask : class, ITask;
	    ITask Get(string name);

		bool TryGet(string name, out ITask task);

	    ITask[] GetAll();
	}
}