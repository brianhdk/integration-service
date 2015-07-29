using System;

namespace Vertica.Integration.Model
{
    internal class TaskExecutionContext : ITaskExecutionContext
    {
        public TaskExecutionContext(ILog log, Arguments arguments)
        {
            if (log == null) throw new ArgumentNullException("log");

            Log = log;
            Arguments = arguments ?? Arguments.Empty;
        }

        public ILog Log { get; private set; }
        public Arguments Arguments { get; private set; }
    }
}