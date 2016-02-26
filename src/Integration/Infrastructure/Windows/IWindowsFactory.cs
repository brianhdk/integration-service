namespace Vertica.Integration.Infrastructure.Windows
{
	public interface IWindowsFactory
	{
		IWindowsServices WindowsServices(string machineName = null);
		ITaskScheduler TaskScheduler(string machineName = null);
	}
}