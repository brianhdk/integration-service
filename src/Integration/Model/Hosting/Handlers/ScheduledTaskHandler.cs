using System;
using System.Reflection;
using System.ServiceProcess;
using Vertica.Integration.Infrastructure.Extensions;
using Vertica.Integration.Infrastructure.Windows;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class ScheduledTaskHandler : IScheduledTaskHandler
	{
		private const string Command = "scheduledTask";

		internal const string ServiceAccountCommand = "account";
		internal const string ServiceAccountUsernameCommand = "username";
		internal const string ServiceAccountPasswordCommand = "password";

		private readonly IRuntimeSettings _runtimeSettings;

		public ScheduledTaskHandler(IRuntimeSettings runtimeSettings)
		{
			_runtimeSettings = runtimeSettings;
		}

		public bool Handle(HostArguments args, ITask task)
		{
			if (args == null) throw new ArgumentNullException("args");
			if (task == null) throw new ArgumentNullException("task");

			string action;
			if (!args.CommandArgs.TryGetValue(Command, out action))
				return false;

			var configuration = new ScheduledTaskConfiguration(task.Name(), Folder(), ExePath, args.Args.ToString())
				.Description(task.Description);

			Func<string, bool> actionIs = arg =>
				String.Equals(arg, action, StringComparison.OrdinalIgnoreCase);

			var installer = new ScheduledTasks();

			if (actionIs("install"))
			{
				string account;
				args.CommandArgs.TryGetValue(ServiceAccountCommand, out account);

				ServiceAccount serviceAccount;
				if (Enum.TryParse(account, out serviceAccount))
					configuration.WithAccount(serviceAccount);

				string username;
				args.CommandArgs.TryGetValue(ServiceAccountUsernameCommand, out username);

				string password;
				args.CommandArgs.TryGetValue(ServiceAccountPasswordCommand, out password);

				if (!String.IsNullOrWhiteSpace(username) && !String.IsNullOrWhiteSpace(password))
					configuration.WithCredentials(username, password);

				installer.InstallOrUpdate(configuration);
			}
			else if (actionIs("uninstall"))
			{
				installer.Uninstall(configuration);
			}

			return true;
		}

		private string Folder()
		{
			ApplicationEnvironment environment = _runtimeSettings.Environment;

			return String.Format("Integration Service{0}",
				environment != null
					? String.Format(" [{0}]", environment)
					: String.Empty);
		}

		private static string ExePath
		{
			get { return Assembly.GetEntryAssembly().Location; }
		}
	}
}