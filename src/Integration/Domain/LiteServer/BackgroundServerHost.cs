using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Castle.MicroKernel;
using Vertica.Integration.Infrastructure;
using Vertica.Utilities;

namespace Vertica.Integration.Domain.LiteServer
{
    internal class BackgroundServerHost : IDisposable
    {
        private readonly IBackgroundServer _server;
        private readonly TaskScheduler _scheduler;
        private readonly Action<string> _output;
        private readonly CancellationToken _token;
        private readonly RestartableContext _context;
        private readonly IUptimeTextGenerator _uptime;

        private Task _current;
        private DateTimeOffset _startedAt;

        private static readonly HashSet<TaskStatus> Running = new HashSet<TaskStatus>(new[]
        {
            TaskStatus.Running,
            TaskStatus.WaitingToRun,
            TaskStatus.WaitingForActivation
        });

        public BackgroundServerHost(IBackgroundServer server, TaskScheduler scheduler, IKernel kernel, Action<string> output, CancellationToken? token = null)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));
            if (output == null) throw new ArgumentNullException(nameof(output));

            _server = server;
            _scheduler = scheduler;
            _token = token.GetValueOrDefault(kernel.Resolve<IShutdown>().Token);
            _output = message => output($"[{this}]: {message}");
            _context = new RestartableContext(kernel);
            _uptime = kernel.Resolve<IUptimeTextGenerator>();
        }

        public void Start()
        {
            if (_current != null)
                throw new InvalidOperationException($"{this} already started.");

            _output("Starting");

            _current = StartTask();

            _output("Started");
        }

        public bool IsRunning(Action<AggregateException> onException, out string statusText)
        {
            if (onException == null) throw new ArgumentNullException(nameof(onException));

            EnsureStarted();

            if (Running.Contains(_current.Status))
            {
                statusText = GetRunningStatusText();
                return true;
            }

            AggregateException exception = _current.Exception;

            if (exception != null)
            {
                onException(exception);
                _context.OnException(exception);

                if (_token.IsCancellationRequested)
                {
                    statusText = "Cancelling";
                    return false;
                }

                if (!(_server is IRestartable restartable))
                {
                    statusText = "Failed";
                    return false;
                }

                _output("Checking whether to restart");

                if (restartable.ShouldRestart(_context))
                {
                    _output("Restarting");

                    _current = StartTask();

                    // We'll simply return true, as the next Housekeeping iteration will tell if the Task is still running.
                    statusText = "Restarted";
                    return true;
                }
            }

            // If the task completed, we'll simply dipose the task thus releasing resources.
            if (_current.IsCompleted)
            {
                _current.Dispose();

                statusText = $"Stopped. Uptime: {_uptime.GetUptimeText(_startedAt)}{(_context.FailedCount > 0 ? $" (Failed: {_context.FailedCount} time(s))" : string.Empty)}";
                return false;
            }

            throw new InvalidOperationException($"{this} has Task with status '{_current.Status}' which was not expected.");
        }

        public string GetRunningStatusText()
        {
            if (_current.Status == TaskStatus.Running)
                return $"Running for {_uptime.GetUptimeText(_startedAt)}{(_context.FailedCount > 0 ? $" (Failed: {_context.FailedCount} time(s))" : string.Empty)}";

            return $"<not running: {_current.Status}>";
        }

        public void WaitForExit(TimeSpan timeout)
        {
            EnsureStarted();

            if (_current.Status == TaskStatus.Running)
            {
                _output("Stopping");

                // Last chance for the task to complete.
                if (_current.Wait(timeout))
                    _output("Stopped");
            }
        }

        public void Dispose()
        {
            if (_current == null)
                return;

            // Running tasks cannot be disposed.
            if (_current.Status != TaskStatus.Running)
                _current.Dispose();
        }

        public override string ToString()
        {
            try
            {
                return _server.ToString();
            }
            catch
            {
                return _server.GetType().Name;
            }
        }

        private void EnsureStarted()
        {
            if (_current == null)
                throw new InvalidOperationException($"{_server} has not been started.");
        }

        private Task StartTask()
        {
            Task task = _server.Create(_context, _token);

            // This will ensure that the task will be run.
            if (task.Status == TaskStatus.Created)
                task.Start(_scheduler);

            _startedAt = Time.UtcNow;

            return task;
        }
    }
}