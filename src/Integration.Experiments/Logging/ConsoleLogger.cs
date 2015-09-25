using System;
using System.IO;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Logging.Loggers;

namespace Vertica.Integration.Experiments.Logging
{
	public class ConsoleLogger : Logger
	{
		private readonly TextWriter _console;

		public ConsoleLogger(TextWriter console)
		{
			_console = console;
		}

		protected override string Insert(TaskLog log)
		{
			throw new NotSupportedException();
		}

		protected override string Insert(MessageLog log)
		{
			throw new NotSupportedException();
		}

		protected override string Insert(StepLog log)
		{
			throw new NotSupportedException();
		}

		protected override string Insert(ErrorLog log)
		{
			_console.WriteLine(log.Message);

			return null;
		}

		protected override void Update(TaskLog log)
		{
			throw new NotImplementedException();
		}

		protected override void Update(StepLog log)
		{
			throw new NotImplementedException();
		}
	}
}