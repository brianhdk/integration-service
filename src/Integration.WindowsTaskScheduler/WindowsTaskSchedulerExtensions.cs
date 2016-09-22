using System;

namespace Vertica.Integration.WindowsTaskScheduler
{
	public static class WindowsTaskSchedulerExtensions
	{
		public static ApplicationConfiguration UseWindowsTaskScheduler(this ApplicationConfiguration application)
		{
			if (application == null) throw new ArgumentNullException(nameof(application));

		    return
		        application.Extensibility(extensibility => 
                    extensibility.Register(() => new WindowsTaskSchedulerConfiguration(application)));
		}
	}
}