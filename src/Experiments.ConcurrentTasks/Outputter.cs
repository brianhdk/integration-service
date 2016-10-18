using System;
using System.IO;
using System.Threading;
using Hangfire;
using Vertica.Integration.Domain.LiteServer;

namespace Experiments.ConcurrentTasks
{
    //public class Outputter : IBackgroundWorker
    //{
    //    private readonly TextWriter _outputter;

    //    public Outputter(TextWriter outputter)
    //    {
    //        _outputter = outputter;
    //    }

    //    public BackgroundWorkerContinuation Work(CancellationToken token, BackgroundWorkerContext context)
    //    {
    //        if (context.InvocationCount > 2)
    //            return context.Exit();

    //        _outputter.WriteLine($"Outputter: {context.InvocationCount}");

    //        return context.Wait(TimeSpan.FromSeconds(5));
    //    }
    //}

    //public class GarbageCollector : IBackgroundWorker
    //{
    //    private readonly TextWriter _outputter;

    //    public GarbageCollector(TextWriter outputter)
    //    {
    //        _outputter = outputter;
    //    }

    //    public BackgroundWorkerContinuation Work(CancellationToken token, BackgroundWorkerContext context)
    //    {
    //        if (context.InvocationCount > 2)
    //            return context.Exit();

    //        _outputter.WriteLine($"GC: {context.InvocationCount}");

    //        GC.Collect();

    //        return context.Wait(TimeSpan.FromSeconds(5));
    //    }
    //}

    //public class RunSynchronousOnlyTaskWithHangfire : IBackgroundWorker
    //{
    //    public BackgroundWorkerContinuation Work(CancellationToken token, BackgroundWorkerContext context)
    //    {
    //        BackgroundJob.Enqueue<ITaskByNameRunner>(x => x.Run(nameof(SynchronousOnlyTask)));

    //        return context.Wait(TimeSpan.FromSeconds(5));
    //    }
    //}

    //public class RunConcurrentExecutionTaskWithHangfire : IBackgroundWorker
    //{
    //    public BackgroundWorkerContinuation Work(CancellationToken token, BackgroundWorkerContext context)
    //    {
    //        BackgroundJob.Enqueue<ITaskByNameRunner>(x => x.Run(nameof(ConcurrentExecutableTask)));

    //        return context.Wait(TimeSpan.FromSeconds(1));
    //    }
    //}
}