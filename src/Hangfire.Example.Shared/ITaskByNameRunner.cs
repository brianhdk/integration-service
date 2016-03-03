namespace Hangfire.Example.Shared
{
	public interface ITaskByNameRunner
	{
		void Run(string taskName);
	}
}
