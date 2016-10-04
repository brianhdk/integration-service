using System.Threading;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Tasks;

namespace Experiments.ConcurrentTasks
{
    [AllowConcurrentExecution]
    public class ConcurrentExecutableTask : Task
    {
        public override void StartTask(ITaskExecutionContext context)
        {
        }

        public override string Description => "This task can be executed in parallel.";
    }

    [PreventConcurrentExecution]
    public class SynchronousOnlyTask : Task
    {
        public override void StartTask(ITaskExecutionContext context)
        {
        }

        public override string Description => "This task cannot be executed in parallel due.";
    }
}