using System;
using System.ServiceProcess;
using System.Text.RegularExpressions;

namespace Vertica.Integration.Infrastructure.Windows
{
	public class Credentials
	{
		public Credentials(ServiceAccount account)
		{
			if (account == ServiceAccount.User) throw new ArgumentException("You must use the constructor that takes username and password when passing a User account.");

			Username = account == ServiceAccount.LocalSystem
				? "NT AUTHORITY\\SYSTEM"
				: $"NT AUTHORITY\\{Regex.Replace(account.ToString(), "[a-z][A-Z]", m => $"{m.Value[0]} {m.Value[1]}")}";

			Account = account;
		}

		public Credentials(string username, string password)
		{
			if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException(@"Value cannot be null or empty", nameof(username));
			if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException(@"Value cannot be null or empty", nameof(password));
			
			Username = username;
			Password = password;
			Account = ServiceAccount.User;
		}

		public ServiceAccount Account { get; private set; }
		public string Username { get; private set; }
		public string Password { get; private set; }
	}
}