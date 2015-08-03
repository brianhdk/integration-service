namespace Vertica.Integration.Model.Hosting.Handlers
{
	public interface IScheduleTaskHandler
	{
		bool Handle(HostArguments args, WindowsService service);
	}
}