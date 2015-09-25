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
			if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");

			_values[name] = value;

			return this;
		}

		public ApplicationEnvironment Environment
		{
			get { return this["Environment"]; }
		}

		public string this[string name]
		{
			get
			{
				if (String.IsNullOrWhiteSpace(name)) throw new ArgumentException(@"Value cannot be null or empty.", "name");

				string value;
				_values.TryGetValue(name, out value);

				return value;
			}
		}
	}
}