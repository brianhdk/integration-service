using System.Collections.Generic;
using System.ServiceProcess;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class InstallWindowsService
	{
		private readonly List<KeyValuePair<string, string>> _commands;

		public InstallWindowsService()
		{
			_commands = new List<KeyValuePair<string, string>>{ WindowsServiceHandler.InstallCommand };
		}

		public InstallWindowsService RunAs(string username, string password)
		{
			CustomCommandArgument(WindowsServiceHandler.ServiceAccountUsernameCommand, username);
			CustomCommandArgument(WindowsServiceHandler.ServiceAccountPasswordCommand, password);

			return this;
		}

		public InstallWindowsService RunAs(ServiceAccount serviceAccount)
		{
			return CustomCommandArgument(WindowsServiceHandler.ServiceAccountCommand, serviceAccount.ToString());
		}

		public InstallWindowsService CustomCommandArgument(string key, string value)
		{
			_commands.Add(new KeyValuePair<string, string>(key, value));

			return this;
		}

		internal KeyValuePair<string, string>[] ToArray()
		{
			return _commands.ToArray();
		}
	}
}