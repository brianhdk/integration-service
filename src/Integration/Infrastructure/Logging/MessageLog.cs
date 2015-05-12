﻿using System;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Logging
{
	public class MessageLog : LogEntry
	{
        internal MessageLog(TaskLog taskLog, string message, Output output)
            : base(taskLog.TaskName, measureExecutionTime: false)
        {
            if (output == null) throw new ArgumentNullException("output");

            Initialize(taskLog, message);
            output.Message("{0}: {1}", taskLog.TaskName, message);
        }

		internal MessageLog(StepLog stepLog, string message, Output output)
            : base(stepLog.TaskName, measureExecutionTime: false)
        {
		    if (output == null) throw new ArgumentNullException("output");

		    Initialize(stepLog, message);
            output.Message("{0}: {1}", stepLog.StepName, message);
        }

		public TaskLog TaskLog { get; private set; }
		public StepLog StepLog { get; private set; }
        public string StepName { get; private set; }
        public string Message { get; private set; }

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

			TaskLog.Persist(this);
		}
	}
}