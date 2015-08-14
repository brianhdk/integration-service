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
		private readonly IScheduleTaskHandler _scheduleTask;

		public TaskHost(ITaskFactory factory, ITaskRunner runner, IWindowsServiceHandler windowsService, IScheduleTaskHandler scheduleTask)
		{
			_factory = factory;
			_runner = runner;
			_windowsService = windowsService;
			_scheduleTask = scheduleTask;
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

			var windowsService = new WindowsService(task.Name(), task.Description).OnStart(() =>
			{
				Action run = () => _runner.Execute(task, args.Args);

				return run.Repeat(args.ParseRepeat(), TextWriter.Null);
			});

			if (!InstallOrRunAsWindowsService(args, windowsService) && !InstallAsScheduleTask(args, task))
				_runner.Execute(task, args.Args);
		}

		private bool InstallOrRunAsWindowsService(HostArguments args, WindowsService windowsService)
		{
			return _windowsService.Handle(args, windowsService);
		}

		private bool InstallAsScheduleTask(HostArguments args, ITask windowsService)
		{
			return _scheduleTask.Handle(windowsService, args);
		}

		public string Description
		{
			get
			{
				return "Handles execution of Tasks.";
			}
		}
	}
}