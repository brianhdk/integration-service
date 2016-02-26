using System;
using System.Configuration.Install;
using System.ServiceProcess;
using Vertica.Utilities_v4.Extensions.StringExt;

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
			WithAccount(ServiceAccount.LocalSystem);
			StartMode(ServiceStartMode.Manual);
		}

		public string ExePath { get; private set; }
		public string Args { get; private set; }

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

		public WindowsServiceConfiguration WithCredentials(string username, string password)
		{
			if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(username));
			if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(password));

			_credentials = new Credentials
			{
				Account = ServiceAccount.User,
				Username = username,
				Password = password
			};

			return this;
		}

		public WindowsServiceConfiguration WithAccount(ServiceAccount account)
		{
			_credentials = new Credentials
			{
				Account = account
			};

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