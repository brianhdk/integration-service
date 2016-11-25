using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.MicroKernel;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Utilities_v4.Extensions.EnumerableExt;
using Scheduler = System.Threading.Tasks.TaskScheduler;

namespace Vertica.Integration.Domain.LiteServer
{
	internal class LiteServerImpl : IDisposable, IBackgroundWorker
	{
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

            Output("Started");
        }

        private void Output(string message)
		{
			_console.WriteLine($"[LiteServer]: {message}.");
		}

		private Task Create(IBackgroundServer server)
		{
			return server.Create(new BackgroundServerContext(), _shutdown.Token);
		}

		public void Dispose()
		{
			Output("Stopping");

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

            // Look into what to do if a task fails - can it be disposed?
            _houseKeeping.Dispose();

			Execute(_configuration.OnShutdown);

			Output("Stopped");
		}

		public BackgroundWorkerContinuation Work(BackgroundWorkerContext context, CancellationToken token)
		{
			if (context.InvocationCount == 1)
			{
                // Ensure all tasks are started.
                foreach (Task nonStartedTask in _tasks.Where(x => x.Status == TaskStatus.Created))
                    nonStartedTask.Start(_scheduler);

                return context.Wait(TimeSpan.FromSeconds(1));
			}

			Task[] failedTasks = _tasks.Where(x => x.IsFaulted).ToArray();

			foreach (Task failedTask in failedTasks)
			{
				if (failedTask.Exception != null)
				{
					foreach (Exception ex in failedTask.Exception.InnerExceptions)
						LogError(ex);
				}

				_tasks.Remove(failedTask);
			}

		    if (token.IsCancellationRequested)
		        return context.Exit();

            if (_tasks.All(x => x.IsCompleted))
			{
				Output("Exiting housekeeping (no more tasks to monitor)");
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