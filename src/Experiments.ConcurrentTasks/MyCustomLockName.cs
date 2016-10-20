using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Tasks;

namespace Experiments.ConcurrentTasks
{
    public class MyCustomLockName : IPreventConcurrentTaskExecutionCustomLockName
    {
        public string GetLockName(ITask currentTask, Arguments arguments)
        {
            return $"{currentTask.Name()}_{arguments["LockNamePostfix"]}";
        }
    }
}