using System;
using System.Threading;
using System.Threading.Tasks;
using Vertica.Integration.Infrastructure.IO;
using Scheduler = System.Threading.Tasks.TaskScheduler;

namespace Vertica.Integration.Domain.LiteServer
{
	internal class BackgroundWorkServer : IBackgroundServer
	{
		private readonly IBackgroundWorker _worker;
		private readonly Scheduler _scheduler;
	    private readonly IConsoleWriter _writer;

		public BackgroundWorkServer(IBackgroundWorker worker, Scheduler scheduler, IConsoleWriter writer)
		{
			if (worker == null) throw new ArgumentNullException(nameof(worker));
			if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
		    if (writer == null) throw new ArgumentNullException(nameof(writer));

		    _worker = worker;
			_scheduler = scheduler;
		    _writer = writer;
		}

		public Task Create(CancellationToken token, BackgroundServerContext context)
		{	
			return Task.Factory.StartNew(() =>
			{
				uint iterations = 0;

				while (!token.IsCancellationRequested)
				{
					BackgroundWorkerContinuation continuation = _worker.Work(token, new BackgroundWorkerContext(++iterations, _writer));

				    TimeSpan waitTime = continuation.WaitTime.GetValueOrDefault();

					if (waitTime <= TimeSpan.Zero)
						break;

					token.WaitHandle.WaitOne(waitTime);
				}

			}, token, TaskCreationOptions.LongRunning, _scheduler);
		}
	}
}