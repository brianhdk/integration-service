using Vertica.Integration.Model;
using Vertica.Integration.Model.Tasks;

namespace Experiments.Console.Tasks.ConcurrentExecution
{
    public class MyCustomLockDescription : IPreventConcurrentTaskExecutionCustomLockDescription
    {
        public string GetLockDescription(ITask currentTask, Arguments arguments, string currentDescription)
        {
            return $"Alias = {arguments["Alias"]}";
        }
    }
}