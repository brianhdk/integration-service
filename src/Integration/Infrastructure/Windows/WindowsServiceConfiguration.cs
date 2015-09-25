using System;
using System.ServiceProcess;
using Vertica.Integration.Model.Hosting.Handlers;

namespace Vertica.Integration.Infrastructure.Windows
{
	public class WindowsServiceConfiguration
	{
		private Credentials _credentials;
		private ServiceStartMode _startMode;

		public WindowsServiceConfiguration()
		{
			WithAccount(ServiceAccount.LocalSystem);
			StartMode(ServiceStartMode.Manual);
		}

		public WindowsServiceConfiguration StartMode(ServiceStartMode startMode)
		{
			_startMode = startMode;

			return this;
		}

		public WindowsServiceConfiguration WithCredentials(string username, string password)
		{
			if (String.IsNullOrWhiteSpace(username)) throw new ArgumentException(@"Value cannot be null or empty.", "username");
			if (String.IsNullOrWhiteSpace(password)) throw new ArgumentException(@"Value cannot be null or empty.", "password");

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
	}
}