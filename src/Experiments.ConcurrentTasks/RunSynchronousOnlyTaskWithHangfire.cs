using System;
using System.Threading;
using Vertica.Integration.Domain.LiteServer;

namespace Experiments.ConcurrentTasks
{
    public class RunSynchronousOnlyTaskWithHangfire : IBackgroundWorker
    {
        public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
        {
            //BackgroundJob.Enqueue<ITaskByNameRunner>(x => x.Run(nameof(SynchronousOnlyTask)));

            return context.Wait(TimeSpan.FromSeconds(30));
        }
    }
}