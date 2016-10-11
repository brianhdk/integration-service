using System;
using System.Threading;
using System.Threading.Tasks;
using Scheduler = System.Threading.Tasks.TaskScheduler;

namespace Vertica.Integration.Domain.LiteServer
{
	internal class BackgroundWorkServer : IBackgroundServer
	{
		private readonly IBackgroundWorker _worker;
		private readonly Scheduler _scheduler;

		public BackgroundWorkServer(IBackgroundWorker worker, Scheduler scheduler)
		{
			if (worker == null) throw new ArgumentNullException(nameof(worker));
			if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));

			_worker = worker;
			_scheduler = scheduler;
		}

		public Task Create(CancellationToken token, BackgroundServerContext context)
		{	
			return Task.Factory.StartNew(() =>
			{
				uint iterations = 0;

				while (!token.IsCancellationRequested)
				{
					BackgroundWorkerContinuation continuation = _worker.Work(token, new BackgroundWorkerContext(++iterations));

				    TimeSpan waitTime = continuation.WaitTime.GetValueOrDefault();

					if (waitTime <= TimeSpan.Zero)
						break;

					token.WaitHandle.WaitOne(waitTime);
				}

			}, token, TaskCreationOptions.LongRunning, _scheduler);
		}
	}
}