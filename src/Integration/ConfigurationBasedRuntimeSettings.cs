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

		public ApplicationEnvironment Environment => this[RuntimeSettings.Environment];
	    public string ApplicationName => this[RuntimeSettings.ApplicationName];
	    public string InstanceName => this[RuntimeSettings.InstanceName];

	    public string this[string name]
		{
			get
			{
				if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(name));

				var configuration = _configuration.Get<RuntimeSettings>();

			    configuration.Values.TryGetValue(name, out string value);

				return value;
			}
		}

		[Guid("8560D663-8E0D-4E27-84D4-9561CA35ED2A")]
		[Description("General purpose configuration used by various tasks, services etc.")]
		public class RuntimeSettings
		{
		    internal const string Environment = nameof(Environment);
		    internal const string ApplicationName = nameof(ApplicationName);
		    internal const string InstanceName = nameof(InstanceName);

			public RuntimeSettings()
			{
			    Values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
			    {
			        [Environment] = string.Empty,
                    [ApplicationName] = string.Empty,
                    [InstanceName] = string.Empty
			    };
			}

			public Dictionary<string,string> Values { get; set; }
		}
	}
}