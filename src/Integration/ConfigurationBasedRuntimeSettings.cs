using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
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
			get { return this[RuntimeSettings.Environment]; }
		}

		public string this[string name]
		{
			get
			{
				if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");

				var configuration = _configuration.Get<RuntimeSettings>();

				string value;
				configuration.Values.TryGetValue(name, out value);

				return value;
			}
		}

		[Guid("8560D663-8E0D-4E27-84D4-9561CA35ED2A")]
		[Description("General purpose configuration used by various tasks, services etc.")]
		public class RuntimeSettings
		{
			internal const string Environment = "Environment";

			public RuntimeSettings()
			{
				Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				Values[Environment] = String.Empty;
			}

			public Dictionary<string,string> Values { get; set; }
		}
	}
}