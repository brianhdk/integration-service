namespace Vertica.Integration.Infrastructure.Windows
{
	public class WindowsFactory : IWindowsFactory
	{
		public IWindowsServices WindowsServices(string machineName = null)
		{
			return new WindowsServices(machineName);
		}

		public ITaskScheduler TaskScheduler(string machineName = null)
		{
			return new TaskScheduler(machineName);
		}
	}
}