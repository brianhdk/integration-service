using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.WindowsTaskScheduler
{
	public interface IScheduledTaskHandler
	{
		bool Handle(HostArguments args, ITask task);
	}
}