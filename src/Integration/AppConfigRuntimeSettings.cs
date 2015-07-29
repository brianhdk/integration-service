using System.Configuration;

namespace Vertica.Integration
{
	public class AppConfigRuntimeSettings : IRuntimeSettings
	{
		public ApplicationEnvironment Environment
		{
			get { return this["Environment"]; }
		}

		public string this[string name]
		{
			get { return ConfigurationManager.AppSettings[name]; }
		}
	}
}