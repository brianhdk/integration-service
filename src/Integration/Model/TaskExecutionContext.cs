using System;
using System.Threading;

namespace Vertica.Integration.Model
{
    internal class TaskExecutionContext : ITaskExecutionContext
    {
        public TaskExecutionContext(ILog log, Arguments arguments, CancellationToken cancellationToken)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
	        if (arguments == null) throw new ArgumentNullException(nameof(arguments));

	        Log = log;
            Arguments = arguments;
            CancellationToken = cancellationToken;
        }

        public ILog Log { get; }
        public Arguments Arguments { get; }

        public CancellationToken CancellationToken { get; }

        public void ThrowIfCancelled()
        {
            CancellationToken.ThrowIfCancellationRequested();
        }
    }
}