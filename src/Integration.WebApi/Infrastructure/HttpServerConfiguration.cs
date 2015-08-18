using System;
using System.Collections.Generic;

namespace Vertica.Integration.WebApi.Infrastructure
{
	public class HttpServerConfiguration
	{
		private readonly List<Action<IOwinConfiguration>> _configurers;

		internal HttpServerConfiguration()
		{
			_configurers = new List<Action<IOwinConfiguration>>();
		}

		public HttpServerConfiguration Configure(Action<IOwinConfiguration> configurer)
		{
			if (configurer == null) throw new ArgumentNullException("configurer");

			_configurers.Add(configurer);

			return this;
		}

		internal void Apply(IOwinConfiguration configuration)
		{
			if (configuration == null) throw new ArgumentNullException("configuration");

			foreach (Action<IOwinConfiguration> configurer in _configurers)
				configurer(configuration);
		}
	}
}