using System;

namespace Vertica.Integration.Domain.LiteServer
{
	internal class InternalConfiguration
	{
		private TimeSpan _shutdownTimeout;

		public InternalConfiguration()
		{
			OnStartup = new StartupActions();
			OnShutdown = new ShutdownActions();
			ShutdownTimeout = TimeSpan.FromSeconds(5);
		    HouseKeepingInterval = TimeSpan.FromSeconds(5);
		}

		public StartupActions OnStartup { get; }
		public ShutdownActions OnShutdown { get; }

		public TimeSpan ShutdownTimeout
		{
			get => _shutdownTimeout;
		    set
			{
				if (value < TimeSpan.Zero)
					throw new ArgumentOutOfRangeException(nameof(value), @"Value must be zero or greater than zero.");

				_shutdownTimeout = value;
			}
		}

        internal TimeSpan HouseKeepingInterval { get; set; }
	}
}