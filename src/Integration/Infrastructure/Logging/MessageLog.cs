using System;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Logging
{
	public class MessageLog : LogEntry
	{
	    private MessageLog(TaskLog taskLog, string message) 
            : base(measureExecutionTime: false)
	    {
	        if (taskLog == null) throw new ArgumentNullException("taskLog");

	        TaskLog = taskLog;
            Message = message.MaxLength(4000);
	    }

	    internal MessageLog(TaskLog taskLog, string message, Output output)
            : this(taskLog, message)
        {
            if (output == null) throw new ArgumentNullException("output");

            output.Message("{0}: {1}", taskLog.Name, message);
        }

		internal MessageLog(StepLog stepLog, string message, Output output)
            : this(stepLog.TaskLog, message)
        {
		    if (output == null) throw new ArgumentNullException("output");

            StepLog = stepLog;

            output.Message("{0}: {1}", stepLog.Name, message);
        }

		public TaskLog TaskLog { get; private set; }
		public StepLog StepLog { get; private set; }
        public string Message { get; private set; }

		public override void Dispose()
		{
			base.Dispose();

			TaskLog.Persist(this);
		}

	    public override string ToString()
	    {
	        return ((LogEntry)StepLog ?? TaskLog).ToString();
	    }
	}
}