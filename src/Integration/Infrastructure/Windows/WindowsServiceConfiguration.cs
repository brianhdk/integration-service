using System;
using System.Configuration.Install;
using System.ServiceProcess;
using Vertica.Utilities.Extensions.StringExt;

namespace Vertica.Integration.Infrastructure.Windows
{
	public class WindowsServiceConfiguration
	{
		private readonly string _serviceName;
		private Credentials _credentials;
		private ServiceStartMode _startMode;
		private string _displayName;
		private string _description;

		public WindowsServiceConfiguration(string serviceName, string exePath, string args = null)
		{
			if (string.IsNullOrWhiteSpace(serviceName)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(serviceName));
			if (string.IsNullOrWhiteSpace(exePath)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(exePath));

			_serviceName = serviceName;
			ExePath = exePath;
			Args = args;
			RunAs(ServiceAccount.LocalSystem);
			StartMode(ServiceStartMode.Manual);
		}

		public string ExePath { get; }
		public string Args { get; }

		public WindowsServiceConfiguration DisplayName(string displayName)
		{
			if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(displayName));

			_displayName = displayName;
			return this;
		}

		public WindowsServiceConfiguration Description(string description)
		{
			_description = description.NullIfEmpty();
			return this;
		}

		public WindowsServiceConfiguration StartMode(ServiceStartMode startMode)
		{
			_startMode = startMode;
			return this;
		}

		public WindowsServiceConfiguration RunAsUser(string username, string password)
		{
			_credentials = new Credentials(username, password);

			return this;
		}

		public WindowsServiceConfiguration RunAs(ServiceAccount account)
		{
			_credentials = new Credentials(account);

			return this;
		}

		internal ServiceInstaller CreateInstaller(ServiceProcessInstaller parent)
		{
			if (parent == null) throw new ArgumentNullException(nameof(parent));

			if (_credentials != null)
			{
				parent.Account = _credentials.Account;
				parent.Username = _credentials.Username;
				parent.Password = _credentials.Password;
			}

			return new ServiceInstaller
			{
				Context = new InstallContext(string.Empty, new[] {$"/assemblypath={ExePath}"}),
				ServiceName = _serviceName,
				DisplayName = _displayName ?? _serviceName,
				Description = _description ?? _displayName ?? _serviceName,
				StartType = _startMode,
				Parent = parent
			};
		}
	}
}