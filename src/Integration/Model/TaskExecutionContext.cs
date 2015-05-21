using System;

namespace Vertica.Integration.Model
{
    internal class TaskExecutionContext : ITaskExecutionContext
    {
        public TaskExecutionContext(ILog log, string[] arguments)
        {
            if (log == null) throw new ArgumentNullException("log");

            Log = log;
            Arguments = arguments ?? new string[0];
        }

        public ILog Log { get; private set; }
        public string[] Arguments { get; private set; }
    }
}