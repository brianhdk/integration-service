using System;
using System.Threading;
using System.Threading.Tasks;
using Castle.MicroKernel;
using Vertica.Integration.Infrastructure;

namespace Vertica.Integration.Domain.LiteServer
{
    internal class BackgroundServerHost : IDisposable
    {
        private readonly IBackgroundServer _server;
        private readonly TaskScheduler _scheduler;
        private readonly Action<string> _output;
        private readonly CancellationToken _token;
        private readonly RestartableContext _context;

        private Task _current;
        
        public BackgroundServerHost(IBackgroundServer server, TaskScheduler scheduler, IKernel kernel, Action<string> output)
        {
            if (server == null) throw new ArgumentNullException(nameof(server));
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));
            if (kernel == null) throw new ArgumentNullException(nameof(kernel));
            if (output == null) throw new ArgumentNullException(nameof(output));

            _server = server;
            _scheduler = scheduler;
            _output = message => output($"[{this}]: {message}");
            _token = kernel.Resolve<IShutdown>().Token;
            _context = new RestartableContext(kernel);
        }

        public void Start()
        {
            if (_current != null)
                throw new InvalidOperationException($"{this} already started.");

            _output("Starting");

            _current = StartTask();

            _output("Started");
        }

        public bool IsRunning(Action<AggregateException> onException)
        {
            if (onException == null) throw new ArgumentNullException(nameof(onException));

            EnsureStarted();

            if (_current.Status == TaskStatus.Running)
                return true;

            AggregateException exception = _current.Exception;

            if (exception != null)
            {
                onException(exception);
                _context.OnException(exception);

                if (_token.IsCancellationRequested)
                    return false;

                var restartable = _server as IRestartable;

                if (restartable == null)
                    return false;

                _output("Checking whether to restart");

                if (restartable.ShouldRestart(_context))
                {
                    _output("Restarting");

                    _current = StartTask();

                    // We'll simply return true, as the next Housekeeping iteration will tell if the Task is still running.
                    return true;
                }
            }

            // If the task completed, we'll simply dipose the task thus releasing resources.
            if (_current.IsCompleted)
            {
                _output("Stopped");

                _current.Dispose();
                return false;
            }

            throw new InvalidOperationException($"{this} has Task with status '{_current.Status}' which was not expected.");
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
            return _server.ToString();
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

            return task;
        }
    }
}