namespace Vertica.Integration.Model.Hosting.Handlers
{
	public interface IScheduledTaskHandler
	{
		bool Handle(HostArguments args, ITask task);
	}
}