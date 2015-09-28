using System;

namespace Vertica.Integration.Infrastructure.Windows
{
	public class ScheduledTaskAction
	{
		public ScheduledTaskAction(string exePath, string args)
		{
			if (String.IsNullOrWhiteSpace(exePath)) throw new ArgumentException(@"Value cannot be null or empty.", "exePath");

			ExePath = exePath;
			Args = args;
		}

		public string ExePath { get; private set; }
		public string Args { get; private set; }
	}
}