using System;
using System.Configuration;

namespace Vertica.Integration
{
	public class AppConfigRuntimeSettings : IRuntimeSettings
	{
		public ApplicationEnvironment Environment => this["Environment"];

		public string this[string name]
		{
			get
			{
				if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(name));

				return ConfigurationManager.AppSettings[name];
			}
		}
	}
}