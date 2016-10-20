using System;
using System.Threading;
using Hangfire;
using Vertica.Integration.Domain.LiteServer;

namespace Experiments.ConcurrentTasks
{
    public class RunConcurrentExecutionTaskWithHangfire : IBackgroundWorker
    {
        public BackgroundWorkerContinuation Work(CancellationToken token, BackgroundWorkerContext context)
        {
            //BackgroundJob.Enqueue<ITaskByNameRunner>(x => x.Run(nameof(ConcurrentExecutableTask)));

            return context.Wait(TimeSpan.FromSeconds(10));
        }
    }
}