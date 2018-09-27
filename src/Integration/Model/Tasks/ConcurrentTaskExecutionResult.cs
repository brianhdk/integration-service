using System;

namespace Vertica.Integration.Model.Tasks
{
    public class ConcurrentTaskExecutionResult : IDisposable
    {
        private readonly IDisposable _lockAcquired;

        private ConcurrentTaskExecutionResult(IDisposable lockAcquired)
        {
            _lockAcquired = lockAcquired;
        }

        private ConcurrentTaskExecutionResult()
        {
            StopTask = true;
        }

        public bool StopTask { get; }

        public static ConcurrentTaskExecutionResult Continue(IDisposable lockAcquired = null)
        {
            return new ConcurrentTaskExecutionResult(lockAcquired);
        }

        public static ConcurrentTaskExecutionResult Stop()
        {
            return new ConcurrentTaskExecutionResult();
        }

        public void Dispose()
        {
            _lockAcquired?.Dispose();
        }
    }
}