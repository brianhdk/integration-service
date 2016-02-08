using Vertica.Integration.Model;

namespace Integration.Hangfire.Services
{
	public class TaskByNameRunner
	{
		private readonly ITaskFactory _factory;
		private readonly ITaskRunner _runner;

		public TaskByNameRunner(ITaskFactory factory, ITaskRunner runner)
		{
			_factory = factory;
			_runner = runner;
		}

		public void Run(string taskName, params string[] args)
		{
			_runner.Execute(_factory.Get(taskName), new Arguments(args));
		}
	}
}