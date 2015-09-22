using System;
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
			get
			{
				if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");

				return ConfigurationManager.AppSettings[name];
			}
		}
	}
}