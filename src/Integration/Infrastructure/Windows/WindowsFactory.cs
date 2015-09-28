namespace Vertica.Integration.Infrastructure.Windows
{
	public class WindowsFactory : IWindowsFactory
	{
		public IWindowsServices CreateWindowsServices(string machineName = null)
		{
			return new WindowsServices(machineName);
		}

		public IScheduledTasks CreateScheduledTasks(string machineName = null)
		{
			return new ScheduledTasks(machineName);
		}
	}
}