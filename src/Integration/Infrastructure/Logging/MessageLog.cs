using System;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Logging
{
	public class MessageLog : LogEntry
	{
		protected MessageLog()
        {
        }

        internal MessageLog(TaskLog taskLog, string message, Output output)
            : base(taskLog.TaskName)
        {
            if (output == null) throw new ArgumentNullException("output");

            Initialize(taskLog, message);
            output.Message("{0}: {1}", taskLog.TaskName, message);
        }

		internal MessageLog(StepLog stepLog, string message, Output output)
            : base(stepLog.TaskName)
        {
		    if (output == null) throw new ArgumentNullException("output");

		    Initialize(stepLog, message);
            output.Message("{0}: {1}", stepLog.StepName, message);
        }

		public virtual TaskLog TaskLog { get; protected set; }
		public virtual StepLog StepLog { get; protected set; }
        public virtual string StepName { get; protected set; }
        public virtual string Message { get; protected set; }

		private void Initialize(TaskLog taskLog, string message)
        {
            TaskLog = taskLog;
            Message = message.MaxLength(4000);
        }

		private void Initialize(StepLog stepLog, string message)
        {
            TaskLog = stepLog.TaskLog;
            StepLog = stepLog;
            StepName = StepLog.StepName;
			Message = message.MaxLength(4000);
        }

		public override void Dispose()
		{
			base.Dispose();

			ExecutionTimeSeconds = 0;

			TaskLog.Persist(this);
		}
	}
}