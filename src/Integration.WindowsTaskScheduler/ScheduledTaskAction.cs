using System;

namespace Vertica.Integration.WindowsTaskScheduler
{
	public class ScheduledTaskAction
	{
		public ScheduledTaskAction(string exePath, string args)
		{
			if (string.IsNullOrWhiteSpace(exePath)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(exePath));

			ExePath = exePath;
			Args = args;
		}

		public string ExePath { get; private set; }
		public string Args { get; private set; }
	}
}