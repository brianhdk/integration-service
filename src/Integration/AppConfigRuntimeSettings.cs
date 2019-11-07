using System;
using System.Configuration;

namespace Vertica.Integration
{
	public class AppConfigRuntimeSettings : IRuntimeSettings
	{
		public ApplicationEnvironment Environment => this[nameof(Environment)];
	    public string ApplicationName => this[nameof(ApplicationName)];
	    public string InstanceName => this[nameof(InstanceName)];

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