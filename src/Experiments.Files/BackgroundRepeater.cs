using System;
using System.Threading;
using System.Threading.Tasks;
using Scheduler = System.Threading.Tasks.TaskScheduler;

namespace Experiments.Files
{
	internal class BackgroundRepeater : IBackgroundOperation
	{
		private readonly IBackgroundRepeatable _repeatable;
		private readonly Scheduler _scheduler;

		public BackgroundRepeater(IBackgroundRepeatable repeatable, Scheduler scheduler)
		{
			if (repeatable == null) throw new ArgumentNullException(nameof(repeatable));
			if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));

			_repeatable = repeatable;
			_scheduler = scheduler;
		}

		public Task Create(CancellationToken token)
		{	
			return Task.Factory.StartNew(() =>
			{
				uint iterations = 0;

				while (!token.IsCancellationRequested)
				{
					TimeSpan waitTime = _repeatable.Work(new BackgroundRepeatedContext(token, ++iterations, () => TimeSpan.MinValue));

					if (waitTime <= TimeSpan.Zero)
						break;

					token.WaitHandle.WaitOne(waitTime);
				}

			}, token, TaskCreationOptions.LongRunning, _scheduler);
		}
	}
}