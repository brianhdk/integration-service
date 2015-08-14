using System;
using System.Collections.Generic;
using System.Web.Http;
using Owin;

namespace Vertica.Integration.WebApi.Infrastructure
{
	public class HttpServerConfiguration
	{
		private readonly List<Action<IAppBuilder, HttpConfiguration>> _configurations;

		internal HttpServerConfiguration()
		{
			_configurations = new List<Action<IAppBuilder, HttpConfiguration>>();
		}

		public HttpServerConfiguration Configure(Action<IAppBuilder, HttpConfiguration> configuration)
		{
			if (configuration == null) throw new ArgumentNullException("configuration");

			_configurations.Add(configuration);

			return this;
		}

		internal void Apply(IAppBuilder app, HttpConfiguration httpConfig)
		{
			foreach (Action<IAppBuilder, HttpConfiguration> configuration in _configurations)
				configuration(app, httpConfig);
		}
	}
}