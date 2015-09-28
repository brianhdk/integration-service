using System;
using System.IO;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Model.Hosting.Handlers;
using Action = System.Action;

namespace Vertica.Integration.Model.Hosting
{
	public class TaskHost : IHost
	{
		private readonly ITaskFactory _factory;
		private readonly ITaskRunner _runner;
		private readonly IWindowsServiceHandler _windowsService;
		private readonly IScheduledTaskHandler _scheduledTask;
		private readonly TextWriter _textWriter;

		public TaskHost(ITaskFactory factory, ITaskRunner runner, IWindowsServiceHandler windowsService, IScheduledTaskHandler scheduledTask, TextWriter textWriter)
		{
			_factory = factory;
			_runner = runner;
			_windowsService = windowsService;
			_scheduledTask = scheduledTask;
			_textWriter = textWriter;
		}

		public bool CanHandle(HostArguments args)
		{
			if (args == null) throw new ArgumentNullException("args");

			return !String.IsNullOrWhiteSpace(args.Command) && _factory.Exists(args.Command);
		}

		public void Handle(HostArguments args)
		{
			if (args == null) throw new ArgumentNullException("args");

			ITask task = _factory.Get(args.Command);

			if (InstallOrRunAsWindowsService(args, task))
				return;

			if (InstallAsScheduledTask(args, task))
				return;

			_runner.Execute(task, args.Args);
		}

		private bool InstallOrRunAsWindowsService(HostArguments args, ITask task)
		{
			Func<IDisposable> onStart = () =>
			{
				string value;
				args.CommandArgs.TryGetValue("interval", out value);

				TimeSpan interval;
				if (!TimeSpan.TryParse(value, out interval))
					interval = TimeSpan.FromMinutes(1);

				return interval.Repeat(() => _runner.Execute(task, args.Args), _textWriter);
			};

			return _windowsService.Handle(args, new HandleAsWindowsService(task.Name(), task.Name(), task.Description, onStart));
		}

		private bool InstallAsScheduledTask(HostArguments args, ITask task)
		{
			return _scheduledTask.Handle(args, task);
		}

		public string Description
		{
			get { return "Handles execution of Tasks."; }
		}
	}
}