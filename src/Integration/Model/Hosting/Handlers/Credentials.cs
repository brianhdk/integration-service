using System.ServiceProcess;

namespace Vertica.Integration.Model.Hosting.Handlers
{
	public class Credentials
	{
		public Credentials()
		{
		}

		public Credentials(string username, string password, ServiceAccount account)
		{
			Username = username;
			Password = password;
			Account = account;
		}

		public ServiceAccount Account { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
	}
}