using System;
using System.Collections.Generic;
using System.Reflection;
using Vertica.Integration.Infrastructure.Extensions;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class ScheduleTaskHandler : IScheduleTaskHandler
	{
		private const string ScheduleTaskCommand = "scheduleTask";

		public bool Handle(ITask task, HostArguments args)
		{
			if (args == null) throw new ArgumentNullException("args");
			if (task == null) throw new ArgumentNullException("task");

			string action;
			string taskFolderName;

			if (!args.CommandArgs.TryGetValue(ScheduleTaskCommand, out action))
				return false;

			args.CommandArgs.TryGetValue("folder", out taskFolderName);
			var scheduleTask = new ScheduleTask(task, taskFolderName);

			Func<string, bool> actionIs = arg =>
				String.Equals(arg, action, StringComparison.OrdinalIgnoreCase);

			var installer = new ScheduleTaskInstaller();

			if (actionIs("install"))
			{
				Install(installer, scheduleTask, args);
			}
			else if (actionIs("uninstall"))
			{
				installer.Uninstall(scheduleTask);
			}

			return true;
		}

		private static void Install(ScheduleTaskInstaller installer, ScheduleTask scheduleTask, HostArguments args)
		{
			string username;
			args.CommandArgs.TryGetValue("username", out username);

			string password;
			args.CommandArgs.TryGetValue("password", out password);

			if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
				scheduleTask.WithCredentials(username, password);

			AddAction(scheduleTask, args);

			installer.InstallOrUpdate(scheduleTask);
		}

		private static void AddAction(ScheduleTask scheduleTask, HostArguments args)
		{
			string exePath;
			if (!args.CommandArgs.TryGetValue("exePath", out exePath))
				exePath = Assembly.GetEntryAssembly().Location;

			scheduleTask.Actions.Add(new Action(exePath, args.Args.ToString()));
		}
	}
}