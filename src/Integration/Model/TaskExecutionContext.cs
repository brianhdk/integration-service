using System;

namespace Vertica.Integration.Model
{
    internal class TaskExecutionContext : ITaskExecutionContext
    {
        public TaskExecutionContext(ILog log, Arguments arguments)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));
	        if (arguments == null) throw new ArgumentNullException(nameof(arguments));

	        Log = log;
            Arguments = arguments;
        }

        public ILog Log { get; }
        public Arguments Arguments { get; }
    }
}