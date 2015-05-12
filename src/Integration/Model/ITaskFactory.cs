namespace Vertica.Integration.Model
{
	public interface ITaskFactory
	{
	    ITask Get<TTask>() where TTask : ITask;
	    ITask GetByName(string name);

	    ITask[] GetAll();
	}
}