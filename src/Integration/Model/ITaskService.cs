namespace Vertica.Integration.Model
{
	public interface ITaskService
	{
        void Execute(string taskName, params string[] arguments);

	    ITask GetByName(string taskName);
        ITask[] GetAll();
	}
}