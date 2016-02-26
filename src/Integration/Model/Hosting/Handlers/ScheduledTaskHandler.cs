using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Windows;
using Vertica.Utilities_v4.Extensions.EnumerableExt;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class ScheduledTaskHandler : IScheduledTaskHandler
	{
		private const string Command = "scheduledTask";

		internal const string ServiceAccountCommand = "account";
		internal const string ServiceAccountUsernameCommand = "username";
		internal const string ServiceAccountPasswordCommand = "password";

		private readonly IRuntimeSettings _runtimeSettings;
		private readonly ITaskScheduler _taskScheduler;

		public ScheduledTaskHandler(IRuntimeSettings runtimeSettings, IWindowsFactory windows)
		{
			_runtimeSettings = runtimeSettings;
			_taskScheduler = windows.TaskScheduler();
		}

		public bool Handle(HostArguments args, ITask task)
		{
			if (args == null) throw new ArgumentNullException(nameof(args));
			if (task == null) throw new ArgumentNullException(nameof(task));

			string action;
			if (!args.CommandArgs.TryGetValue(Command, out action))
				return false;

			var configuration = new ScheduledTaskConfiguration(task.Name(), Folder(), ExePath, ExeArgs(args))
				.Description(task.Description);

			Func<KeyValuePair<string, string>, bool> actionIs = command =>
				string.Equals(command.Value, action, StringComparison.OrdinalIgnoreCase);

			if (actionIs(InstallCommand))
			{
				string account;
				if (args.CommandArgs.TryGetValue(ServiceAccountCommand, out account))
				{
					ServiceAccount serviceAccount;
					if (Enum.TryParse(account, out serviceAccount))
						configuration.RunAs(serviceAccount);
				}
				else
				{
					string username;
					args.CommandArgs.TryGetValue(ServiceAccountUsernameCommand, out username);

					string password;
					args.CommandArgs.TryGetValue(ServiceAccountPasswordCommand, out password);

					if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
						configuration.RunAsUser(username, password);
				}

				_taskScheduler.InstallOrUpdate(configuration);
			}
			else if (actionIs(UninstallCommand))
			{
				_taskScheduler.Uninstall(configuration.Name, configuration.Folder);
			}

			return true;
		}

		private string Folder()
		{
			ApplicationEnvironment environment = _runtimeSettings.Environment;

			return $"Integration Service{(environment != null ? $" [{environment}]" : string.Empty)}";
		}

		private static string ExePath => Assembly.GetEntryAssembly().Location;

		private static string ExeArgs(HostArguments args)
		{
			Arguments arguments = new Arguments(args.CommandArgs
				.Where(x => !ReservedCommandArgs.Contains(x.Key, StringComparer.OrdinalIgnoreCase))
				.Append(args.Args.ToArray())
				.ToArray());

			return $"{args.Command} {arguments}";
		}


		private static IEnumerable<string> ReservedCommandArgs
		{
			get
			{
				yield return Command;
				yield return ServiceAccountCommand;
				yield return ServiceAccountUsernameCommand;
				yield return ServiceAccountPasswordCommand;
			}
		}

		public static KeyValuePair<string, string> InstallCommand => new KeyValuePair<string, string>(Command, "install");
		public static KeyValuePair<string, string> UninstallCommand => new KeyValuePair<string, string>(Command, "uninstall");
	}
}