using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Logging
{
	public class StepLog : LogEntry
	{
        private readonly Output _output;

		private readonly IList<MessageLog> _messages;

		internal StepLog(TaskLog taskLog, string stepName, Output output)
			: base(taskLog.TaskName)
		{
			if (taskLog == null) throw new ArgumentNullException("taskLog");
		    if (output == null) throw new ArgumentNullException("output");

		    _output = output;

			_messages = new List<MessageLog>();

			Initialize(taskLog, stepName);
		}

		public virtual TaskLog TaskLog { get; private set; }
		public virtual string StepName { get; private set; }
		public virtual ErrorLog ErrorLog { get; internal set; }

		public virtual ReadOnlyCollection<MessageLog> Messages
		{
			get { return new ReadOnlyCollection<MessageLog>(_messages); }
		}

		private void Initialize(TaskLog taskLog, string stepName)
		{
			TaskLog = taskLog;
			StepName = stepName;

			TaskLog.Persist(this);
		    _output.Message(StepName);
		}

		public virtual void LogMessage(string message)
		{
			using (var log = new MessageLog(this, message, _output))
			{
				_messages.Add(log);
			}
		}

		public override void Dispose()
		{
			base.Dispose();

			TaskLog.Persist(this);
		}
	}
}