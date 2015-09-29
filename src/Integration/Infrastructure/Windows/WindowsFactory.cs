namespace Vertica.Integration.Infrastructure.Windows
{
	public class WindowsFactory : IWindowsFactory
	{
		public IWindowsServices WindowsServices(string machineName = null)
		{
			return new WindowsServices(machineName);
		}

		public IScheduledTasks ScheduledTasks(string machineName = null)
		{
			return new ScheduledTasks(machineName);
		}
	}
}