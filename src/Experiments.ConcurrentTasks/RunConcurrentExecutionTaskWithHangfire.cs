using System;
using System.Threading;
using Hangfire;
using Vertica.Integration.Domain.LiteServer;

namespace Experiments.ConcurrentTasks
{
    public class RunConcurrentExecutionTaskWithHangfire : IBackgroundWorker
    {
        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            //BackgroundJob.Enqueue<ITaskByNameRunner>(x => x.Run(nameof(ConcurrentExecutableTask)));

            return context.Wait(TimeSpan.FromSeconds(10));
        }
    }
}