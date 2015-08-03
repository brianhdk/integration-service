using System;
using System.Reflection;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class ScheduleTaskHandler : IScheduleTaskHandler
	{
		public bool Handle(HostArguments args, WindowsService service)
		{
			if (args == null) throw new ArgumentNullException("args");
			if (service == null) throw new ArgumentNullException("service");

			string action;
			string taskFolderName;

			if (!args.CommandArgs.TryGetValue("scheduleTask", out action))
				return false;

			if (!args.CommandArgs.TryGetValue("folder", out taskFolderName))
				taskFolderName = "Integration Service";

			var scheduleTask = new ScheduleTask(service.Name, service.Description, taskFolderName);

			Func<string, bool> actionIs = arg =>
				String.Equals(arg, action, StringComparison.OrdinalIgnoreCase);

			var installer = new ScheduleTaskInstaller();

			if (actionIs("install"))
			{
				Install(args, scheduleTask, installer);
			}
			else if (actionIs("uninstall"))
			{
				installer.Uninstall(scheduleTask);
			}

			return true;
		}

		private static void Install(HostArguments args, ScheduleTask scheduleTask, ScheduleTaskInstaller installer)
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