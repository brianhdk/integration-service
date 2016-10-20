using System.Threading;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Tasks;

namespace Experiments.ConcurrentTasks
{
    [AllowConcurrentTaskExecution]
    public class ConcurrentExecutableTask : Task
    {
        public override void StartTask(ITaskExecutionContext context)
        {
            int iterations = 0;

            while (!context.CancellationToken.IsCancellationRequested)
            {
                if (++iterations >= 5)
                    return;

                context.Log.Message($"Running {iterations}.");
                context.CancellationToken.WaitHandle.WaitOne(1000);
            }
        }

        public override string Description => "This task can be executed in parallel.";
    }

    [PreventConcurrentTaskExecution(RuntimeEvaluator = typeof(MyCustomEvaluator), CustomLockName = typeof(MyCustomLockName))]
    public class SynchronousOnlyTask : Task
    {
        public override void StartTask(ITaskExecutionContext context)
        {
            int iterations = 0;

            while (!context.CancellationToken.IsCancellationRequested)
            {
                if (++iterations >= 3)
                    break;

                context.Log.Message($"Running {iterations} - {Thread.CurrentThread.ManagedThreadId}.");
                context.CancellationToken.WaitHandle.WaitOne(1000);
            }

            context.Log.Message($"Done: {Thread.CurrentThread.ManagedThreadId}");
        }

        public override string Description => "This task cannot be executed in parallel.";
    }
}