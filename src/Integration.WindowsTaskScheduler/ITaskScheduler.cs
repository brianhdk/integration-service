namespace Vertica.Integration.WindowsTaskScheduler
{
	public interface ITaskScheduler
	{
		void InstallOrUpdate(ScheduledTaskConfiguration scheduledTask);
		void Uninstall(string name, string folder);
	}
}