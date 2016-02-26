using System;
using System.Collections.Generic;

namespace Vertica.Integration
{
	public class InMemoryRuntimeSettings : IRuntimeSettings
	{
		private readonly IDictionary<string, string> _values;

		public InMemoryRuntimeSettings(IDictionary<string, string> values = null)
		{
			_values = values ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		}

		public InMemoryRuntimeSettings Set(string name, string value)
		{
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(name));

			_values[name] = value;

			return this;
		}

		public ApplicationEnvironment Environment => this["Environment"];

		public string this[string name]
		{
			get
			{
				if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", nameof(name));

				string value;
				_values.TryGetValue(name, out value);

				return value;
			}
		}
	}
}