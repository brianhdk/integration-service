using System;
using System.Diagnostics;
using Vertica.Utilities_v4;

namespace Vertica.Integration.Infrastructure.Logging
{
    public abstract class LogEntry : IDisposable
    {
        private readonly Stopwatch _watch;

        protected LogEntry(bool measureExecutionTime = true)
        {
            _watch = new Stopwatch();

            if (measureExecutionTime)
                _watch.Start();

            TimeStamp = Time.UtcNow;
        }

        public string Id { get; internal set; }
        public double? ExecutionTimeSeconds { get; private set; }
        public DateTimeOffset TimeStamp { get; private set; }

        public virtual void Dispose()
        {
            if (_watch.IsRunning)
            {
                _watch.Stop();

                ExecutionTimeSeconds = _watch.Elapsed.TotalSeconds;
            }
        }
    }
}