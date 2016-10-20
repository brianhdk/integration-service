using System;
using System.Threading;
using Hangfire;
using Vertica.Integration.Domain.LiteServer;

namespace Experiments.ConcurrentTasks
{
    public class RunSynchronousOnlyTaskWithHangfire : IBackgroundWorker
    {
        public BackgroundWorkerContinuation Work(CancellationToken token, BackgroundWorkerContext context)
        {
            //BackgroundJob.Enqueue<ITaskByNameRunner>(x => x.Run(nameof(SynchronousOnlyTask)));

            return context.Wait(TimeSpan.FromSeconds(30));
        }
    }
}