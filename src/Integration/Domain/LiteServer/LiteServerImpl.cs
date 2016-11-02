using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.MicroKernel;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Extensions.EnumerableExt;
using Scheduler = System.Threading.Tasks.TaskScheduler;

namespace Vertica.Integration.Domain.LiteServer
{
	internal class LiteServerImpl : IDisposable, IBackgroundWorker
	{
		private readonly DateTimeOffset _started;

	    private readonly IKernel _kernel;
	    private readonly IShutdown _shutdown;
	    private readonly ILogger _logger;
	    private readonly IConsoleWriter _console;
	    private readonly InternalConfiguration _configuration;

	    private readonly Scheduler _scheduler;
		private readonly List<Task> _tasks;
		private readonly Task _houseKeeping;

		public LiteServerImpl(IKernel kernel, InternalConfiguration configuration)
		{
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));

			_started = Time.UtcNow;

		    _kernel = kernel;
		    _shutdown = kernel.Resolve<IShutdown>();
		    _console = kernel.Resolve<IConsoleWriter>();
		    _logger = kernel.Resolve<ILogger>();
		    _configuration = configuration;

		    _scheduler = Scheduler.Current;

			Output("Starting");

			Execute(_configuration.OnStartup);

			_tasks = kernel.ResolveAll<IBackgroundWorker>()
				.Select(worker => new BackgroundWorkServer(worker, _scheduler, _console))
				.Concat(kernel.ResolveAll<IBackgroundServer>())
				.Select(Create)
                .SkipNulls()
				.ToList();

			_houseKeeping = Create(new BackgroundWorkServer(this, _scheduler, _console));
		}

		private void Output(string message)
		{
			_console.WriteLine($"[LiteServer]: {message}");
		}

		private Task Create(IBackgroundServer server)
		{
			return server.Create(_shutdown.Token, new BackgroundServerContext());
		}

		public void Dispose()
		{
			Output("Stopping.");

			Exception[] exceptions = _tasks
				.Where(x => x.IsFaulted && x.Exception != null)
				.SelectMany(x => x.Exception.Flatten().InnerExceptions)
				.ToArray();

			try
			{
				Task[] waitTasks = _tasks
					.Where(x => !x.IsFaulted)
					.Append(_houseKeeping)
					.ToArray();

				// We allow for some wait-time to finish the background threads.
				Task.WaitAll(waitTasks, _configuration.ShutdownTimeout);
			}
			catch (AggregateException ex)
			{
				exceptions = exceptions
					.Concat(ex.Flatten().InnerExceptions)
					.ToArray();
			}

			foreach (Exception ex in exceptions)
				LogError(ex);

			_houseKeeping.Dispose();

			Execute(_configuration.OnShutdown);

			Output($"Stopped. Uptime: {Uptime}.");
		}

		private string Uptime
		{
			get
			{
				TimeSpan span = Time.UtcNow - _started;

				if (span.TotalSeconds < 1)
					return $"{span.TotalSeconds} seconds";

				var segments = new List<string>(4);

				if (span.Days > 0)
					segments.Add($"{span.Days} day{(span.Days == 1 ? string.Empty : "s")}");

				if (span.Hours > 0)
					segments.Add($"{span.Hours} hour{(span.Hours == 1 ? string.Empty : "s")}");

				if (span.Minutes > 0)
					segments.Add($"{span.Minutes} minute{(span.Minutes == 1 ? string.Empty : "s")}");

				if (span.Seconds > 0)
					segments.Add($"{span.Seconds} second{(span.Seconds == 1 ? string.Empty : "s")}");

				return string.Join(" ", segments);
			}
		}

		public BackgroundWorkerContinuation Work(CancellationToken token, BackgroundWorkerContext context)
		{
			// Ensure all tasks are started.
			foreach (Task nonStartedTask in _tasks.Where(x => x.Status == TaskStatus.Created))
				nonStartedTask.Start(_scheduler);

			if (context.InvocationCount == 1)
				return context.Wait(TimeSpan.FromSeconds(1));

			Task[] failedTasks = _tasks.Where(x => x.IsFaulted).ToArray();

			foreach (Task failedTask in failedTasks)
			{
				if (token.IsCancellationRequested)
					break;

				if (failedTask.Exception != null)
				{
					foreach (Exception ex in failedTask.Exception.InnerExceptions)
						LogError(ex);
				}

				_tasks.Remove(failedTask);
			}

			if (_tasks.All(x => x.IsCompleted))
			{
				Output("Exiting housekeeping (no more tasks to monitor).");
				return context.Exit();
			}

			return context.Wait(TimeSpan.FromSeconds(5));
		}

		private void LogError(Exception ex)
		{
			Output($"ERROR: {ex.Message}");
			_logger.LogError(ex);
		}

		private void Execute(IEnumerable<Action<IKernel>> actions)
		{
			foreach (Action<IKernel> action in actions)
				action(_kernel);
		}
	}
}