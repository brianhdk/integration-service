using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Logging
{
	public class StepLog : LogEntry
	{
        private readonly Output _output;
		private readonly IList<MessageLog> _messages;

		internal StepLog(TaskLog taskLog, IStep step, Output output)
			: base(taskLog.TaskName)
		{
			if (taskLog == null) throw new ArgumentNullException("taskLog");
		    if (step == null) throw new ArgumentNullException("step");
		    if (output == null) throw new ArgumentNullException("output");

		    _output = output;
			_messages = new List<MessageLog>();

            TaskLog = taskLog;
		    StepName = step.Name();

            TaskLog.Persist(this);
            _output.Message(StepName);
		}

		public TaskLog TaskLog { get; private set; }
		public string StepName { get; private set; }
		public ErrorLog ErrorLog { get; internal set; }

		public ReadOnlyCollection<MessageLog> Messages
		{
			get { return new ReadOnlyCollection<MessageLog>(_messages); }
		}

		public void LogMessage(string message)
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