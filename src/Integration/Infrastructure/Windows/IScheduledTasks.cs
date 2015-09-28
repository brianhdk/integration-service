namespace Vertica.Integration.Infrastructure.Windows
{
	public interface IScheduledTasks
	{
		void InstallOrUpdate(ScheduledTaskConfiguration scheduledTask);
		void Uninstall(string name, string folder);
	}
}