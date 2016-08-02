using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.MicroKernel;
using Vertica.Integration.Infrastructure.Logging;
using Scheduler = System.Threading.Tasks.TaskScheduler;

namespace Experiments.Files
{
	internal class Server : IDisposable, IBackgroundRepeatable
	{
		private readonly IKernel _kernel;
		private readonly InternalConfiguration _configuration;
		private readonly ILogger _logger;
		private readonly TextWriter _outputter;

		private readonly CancellationTokenSource _cancellation;
		private readonly Scheduler _scheduler;
		private readonly List<Task> _tasks;

		public Server(IKernel kernel, InternalConfiguration configuration)
		{
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));

			_kernel = kernel;
			_configuration = configuration;
			_outputter = kernel.Resolve<TextWriter>();
			_logger = kernel.Resolve<ILogger>();

			_cancellation = new CancellationTokenSource();
			_scheduler = Scheduler.Current;

			_outputter.WriteLine("Starting Server");
			_outputter.WriteLine();

			Execute(_configuration.OnStartup);

			_tasks = _kernel.ResolveAll<IBackgroundRepeatable>()
				.Select(repeatable => new BackgroundRepeater(repeatable, _scheduler))
				.Concat(_kernel.ResolveAll<IBackgroundOperation>())
				.Select(Create)
				.ToList();

			// We'll add our own instance for house-keeping operations.
			_tasks.Add(Create(new BackgroundRepeater(this, _scheduler)));
		}

		private Task Create(IBackgroundOperation operation)
		{
			return operation.Create(_cancellation.Token);
		}

		public void Dispose()
		{
			_outputter.WriteLine("Stopping server.");
			_outputter.WriteLine();

			// Signal to cancel all tasks.
			_cancellation.Cancel();

			Exception[] exceptions = _tasks
				.Where(x => x.IsFaulted && x.Exception != null)
				.SelectMany(x => x.Exception.Flatten().InnerExceptions)
				.ToArray();

			try
			{
				// We allow for some wait-time to finish the background threads.
				Task.WaitAll(_tasks.Where(x => !x.IsFaulted).ToArray(), _configuration.WaitOnTasksTimeout);
			}
			catch (AggregateException ex)
			{
				exceptions = exceptions
					.Concat(ex.Flatten().InnerExceptions)
					.ToArray();
			}

			foreach (Exception ex in exceptions)
				_logger.LogError(ex);

			_cancellation.Dispose();

			Execute(_configuration.OnShutdown);
		}

		public TimeSpan Work(BackgroundRepeatedContext context)
		{
			// Ensure all tasks are started.
			foreach (Task nonStartedTask in _tasks.Where(x => x.Status == TaskStatus.Created))
				nonStartedTask.Start(_scheduler);

			foreach (Task failedTask in _tasks.Where(x => x.IsFaulted))
			{
				if (context.CancellationToken.IsCancellationRequested)
					break;

				if (failedTask.Exception != null)
				{
					foreach (Exception ex in failedTask.Exception.InnerExceptions)
						_logger.LogError(ex);
				}
			}

			return TimeSpan.FromSeconds(5);
		}
		
		private void Execute(IEnumerable<Action<IKernel>> actions)
		{
			foreach (Action<IKernel> action in actions)
				action(_kernel);
		}
	}
}