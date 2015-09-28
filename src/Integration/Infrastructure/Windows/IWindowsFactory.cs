namespace Vertica.Integration.Infrastructure.Windows
{
	public interface IWindowsFactory
	{
		IWindowsServices CreateWindowsServices(string machineName = null);
		IScheduledTasks CreateScheduledTasks(string machineName = null);
	}
}