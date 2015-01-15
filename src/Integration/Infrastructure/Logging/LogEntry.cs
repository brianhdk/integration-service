using System;
using System.Diagnostics;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Logging
{
    public abstract class LogEntry : IDisposable
    {
        private readonly Stopwatch _watch;

        public virtual int Id { get; protected set; }
        public virtual string TaskName { get; protected set; }
        public virtual double ExecutionTimeSeconds { get; protected set; }
        public virtual DateTimeOffset TimeStamp { get; protected set; }

        protected LogEntry()
        {
        }

        protected LogEntry(string taskName, bool startStopwatch = true)
        {
			_watch = new Stopwatch();

			if (startStopwatch)
				_watch.Start();

			Initialize(taskName);
        }

        private void Initialize(string taskName)
        {
            TaskName = taskName;
			TimeStamp = Time.UtcNow;
        }

	    public virtual void Dispose()
        {
			if (_watch.IsRunning)
				_watch.Stop();

            ExecutionTimeSeconds = _watch.Elapsed.TotalSeconds;
        }
    }
}