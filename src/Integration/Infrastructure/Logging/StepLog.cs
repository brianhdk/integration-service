using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model;

namespace Vertica.Integration.Infrastructure.Logging
{
	public class StepLog : LogEntry, IReferenceErrorLog
	{
        private readonly Output _output;
		private readonly IList<MessageLog> _messages;

		internal StepLog(TaskLog taskLog, IStep step, Output output)
		{
			if (taskLog == null) throw new ArgumentNullException(nameof(taskLog));
		    if (step == null) throw new ArgumentNullException(nameof(step));
		    if (output == null) throw new ArgumentNullException(nameof(output));

		    _output = output;
			_messages = new List<MessageLog>();

            TaskLog = taskLog;
		    Name = step.Name();

            TaskLog.Persist(this);
            _output.Message(Name);
		}

		public TaskLog TaskLog { get; }
		public string Name { get; }

		public ErrorLog ErrorLog { get; internal set; }

		public ReadOnlyCollection<MessageLog> Messages => new ReadOnlyCollection<MessageLog>(_messages);

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

        public override string ToString()
        {
            return Name;
        }
	}
}