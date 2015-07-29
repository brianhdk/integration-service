using System.Configuration;

namespace Vertica.Integration
{
	public class AppSettingsProvider : ISettingsProvider
	{
		public string Environment
		{
			get { return this["Environment"]; }
		}

		public string this[string name]
		{
			get { return ConfigurationManager.AppSettings[name]; }
		}
	}
}