using System;

namespace Experiments.Files
{
	internal class InternalConfiguration
	{
		private TimeSpan _waitOnTasksTimeout;

		public InternalConfiguration()
		{
			OnStartup = new StartupActions();
			OnShutdown = new ShutdownActions();
			WaitOnTasksTimeout = TimeSpan.FromSeconds(5);
		}

		public StartupActions OnStartup { get; }
		public ShutdownActions OnShutdown { get; }

		public TimeSpan WaitOnTasksTimeout
		{
			get { return _waitOnTasksTimeout; }
			set
			{
				if (value < TimeSpan.Zero)
					throw new ArgumentOutOfRangeException(nameof(value), @"Value must be zero or greater than zero.");

				_waitOnTasksTimeout = value;
			}
		}
	}
}