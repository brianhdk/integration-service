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

		public Task Create(CancellationToken token)
		{	
			return Task.Factory.StartNew(() =>
			{
				uint iterations = 0;

				while (!token.IsCancellationRequested)
				{
					TimeSpan waitTime = _worker.Work(new BackgroundWorkContext(token, ++iterations, () => TimeSpan.MinValue));

					if (waitTime <= TimeSpan.Zero)
						break;

					token.WaitHandle.WaitOne(waitTime);
				}

			}, token, TaskCreationOptions.LongRunning, _scheduler);
		}
	}
}