namespace Vertica.Integration.Infrastructure.Windows
{
	public interface ITaskScheduler
	{
		void InstallOrUpdate(ScheduledTaskConfiguration scheduledTask);
		void Uninstall(string name, string folder);
	}
}