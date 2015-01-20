using System.Collections.Generic;

namespace Vertica.Integration.Model
{
	public interface ITaskFactory
	{
	    ITask GetTaskByName(string name);

	    IEnumerable<ITask> GetTasks();
	}
}