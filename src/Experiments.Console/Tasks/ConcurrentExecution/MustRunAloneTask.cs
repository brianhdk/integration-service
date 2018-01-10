using System;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Tasks;

namespace Experiments.Console.Tasks.ConcurrentExecution
{
    [PreventConcurrentTaskExecution(CustomLockDescription = typeof(MyCustomLockDescription))]
    public class MustRunAloneTask : IntegrationTask
    {
        public override void StartTask(ITaskExecutionContext context)
        {
            context.Log.Message("Alias {0} will finish in 3 seconds...", context.Arguments["Alias"]);
            context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromSeconds(3));
            context.Log.Message("Alias {0} finished...", context.Arguments["Alias"]);
        }

        public override string Description => nameof(MustRunAloneTask);
    }
}