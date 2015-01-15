using System.Collections.Generic;

namespace Vertica.Integration.Model
{
	public interface ITaskService
	{
        void Execute(string taskName, params string[] arguments);

        IEnumerable<ITask> GetAll();
	}
}