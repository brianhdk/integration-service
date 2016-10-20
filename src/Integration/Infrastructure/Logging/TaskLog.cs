using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Windows;
using Vertica.Integration.Model;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Infrastructure.Logging
{
    public class TaskLog : LogEntry, IReferenceErrorLog
	{
		private readonly Action<LogEntry> _persist;
        private readonly Output _output;

		private readonly IList<StepLog> _steps;
		private readonly IList<MessageLog> _messages;

	    internal TaskLog(ITask task, Action<LogEntry> persist, Output output)
		{
		    if (persist == null) throw new ArgumentNullException(nameof(persist));
		    if (output == null) throw new ArgumentNullException(nameof(output));

		    _persist = persist;
            _output = output;

			_steps = new List<StepLog>();
			_messages = new List<MessageLog>();

	        Name = task.Name();

            MachineName = Environment.MachineName;
            IdentityName = WindowsUtils.GetIdentityName();
            CommandLine = Environment.CommandLine.MaxLength(4000);

			Persist(this);
	        _output.Message(Name);
		}

	    public string Name { get; }

		public string MachineName { get; private set; }
        public string IdentityName { get; private set; }
        public string CommandLine { get; private set; }

		public ReadOnlyCollection<StepLog> Steps => _steps.EmptyIfNull().Any() ? new ReadOnlyCollection<StepLog>(_steps): new ReadOnlyCollection<StepLog>(new List<StepLog>());

	    public ReadOnlyCollection<MessageLog> Messages => _messages.EmptyIfNull().Any() ? new ReadOnlyCollection<MessageLog>(_messages) : new ReadOnlyCollection<MessageLog>(new List<MessageLog>());

	    public ErrorLog ErrorLog { get; internal set; }

		public StepLog LogStep(IStep step)
		{
		    if (step == null) throw new ArgumentNullException(nameof(step));

		    var log = new StepLog(this, step, _output);

			_steps.Add(log);

			return log;
		}

		public void LogMessage(string message)
		{
			using (var log = new MessageLog(this, message, _output))
			{
				_messages.Add(log);
			}
		}

		protected internal void Persist(LogEntry logEntry)
		{
			if (logEntry == null) throw new ArgumentNullException(nameof(logEntry));

			_persist(logEntry);
		}

		public override void Dispose()
		{
			base.Dispose();

			Persist(this);
		}

	    public override string ToString()
	    {
	        return Name;
	    }
	}
}