using System.Collections.Generic;

namespace Vertica.Integration.Model
{
	public interface ITaskFactory
	{
		IEnumerable<ITask> GetTasks();
		ITask GetTaskByName(string name);
	}
}