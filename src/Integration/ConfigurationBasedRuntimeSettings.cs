using System;
using System.Collections.Generic;
using Vertica.Integration.Infrastructure.Configuration;

namespace Vertica.Integration
{
	public class ConfigurationBasedRuntimeSettings : IRuntimeSettings
	{
		private readonly IConfigurationService _configuration;

		public ConfigurationBasedRuntimeSettings(IConfigurationService configuration)
		{
			_configuration = configuration;
		}

		public ApplicationEnvironment Environment
		{
			get { return this["Environment"]; }
		}

		public string this[string name]
		{
			get
			{
				var configuration = _configuration.Get<Configuration>();

				string value;
				configuration.Values.TryGetValue(name, out value);

				return value;
			}
		}

		public class Configuration
		{
			public Configuration()
			{
				Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			}

			public Dictionary<string,string> Values { get; set; }
		}
	}
}