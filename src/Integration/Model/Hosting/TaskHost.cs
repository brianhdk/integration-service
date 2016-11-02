using System;

namespace Vertica.Integration.Model.Hosting
{
	public class TaskHost : IHost
	{
	    private readonly ITaskFactory _factory;
	    private readonly ITaskRunner _runner;

		public TaskHost(ITaskFactory factory, ITaskRunner runner)
		{
		    _factory = factory;
			_runner = runner;
		}

		public bool CanHandle(HostArguments args)
		{
			if (args == null) throw new ArgumentNullException(nameof(args));

			return !string.IsNullOrWhiteSpace(args.Command) && _factory.Exists(args.Command);
		}

		public void Handle(HostArguments args)
		{
			if (args == null) throw new ArgumentNullException(nameof(args));

			ITask task = _factory.Get(args.Command);

			_runner.Execute(task, args.Args);
		}

		public string Description => "Handles execution of Tasks.";
	}
}