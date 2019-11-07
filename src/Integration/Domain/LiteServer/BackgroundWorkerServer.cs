using System;
using System.Threading;
using System.Threading.Tasks;
using Scheduler = System.Threading.Tasks.TaskScheduler;

namespace Vertica.Integration.Domain.LiteServer
{
	internal class BackgroundWorkerServer : IBackgroundServer, IRestartable
	{
		private readonly IBackgroundWorker _worker;
		private readonly Scheduler _scheduler;

		public BackgroundWorkerServer(IBackgroundWorker worker, Scheduler scheduler)
		{
			if (worker == null) throw new ArgumentNullException(nameof(worker));
			if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));

		    _worker = worker;
			_scheduler = scheduler;
		}

		public Task Create(BackgroundServerContext context, CancellationToken token)
		{
		    if (context == null) throw new ArgumentNullException(nameof(context));

		    return Task.Factory.StartNew(() =>
			{
                var workerContext = new BackgroundWorkerContext(context.Console);

                while (!token.IsCancellationRequested)
				{
					BackgroundWorkerContinuation continuation = _worker.Work(workerContext.Increment(), token);

				    TimeSpan waitTime = continuation.WaitTime.GetValueOrDefault();

					if (waitTime <= TimeSpan.Zero)
						break;

					token.WaitHandle.WaitOne(waitTime);
				}

			}, token, TaskCreationOptions.LongRunning, _scheduler);
		}

	    public bool ShouldRestart(RestartableContext context)
	    {
	        if (context == null) throw new ArgumentNullException(nameof(context));

	        var restartable = _worker as IRestartable;

	        return restartable != null && restartable.ShouldRestart(context);
	    }

	    public override string ToString()
	    {
	        return _worker.ToString();
	    }
	}
}