namespace Vertica.Integration.Model
{
	public interface ITaskFactory
	{
	    ITask GetByName(string name);

	    ITask[] GetAll();
	}
}