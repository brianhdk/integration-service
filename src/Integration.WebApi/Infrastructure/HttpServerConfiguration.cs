using System;
using System.Collections.Generic;

namespace Vertica.Integration.WebApi.Infrastructure
{
	public class HttpServerConfiguration
	{
		private readonly List<Action<IOwinConfiguration>> _configurers;
		private bool _allowCaching;
		private bool _allowHttpFormatter;

		internal HttpServerConfiguration()
		{
			_configurers = new List<Action<IOwinConfiguration>>();
		}

		/// <summary>
		/// By default Integration Service will disable caching on all end-points, unless specific cache-control is specified. Use this method to re-enable default behaviour.
		/// </summary>
		public HttpServerConfiguration AllowCaching()
		{
			_allowCaching = true;
			return this;
		}

		/// <summary>
		/// By default Integration Service removes the Xml formatter. Use this method to re-enable the Xml formatter.
		/// </summary>
		public HttpServerConfiguration AllowXmlFormatter()
		{
			_allowHttpFormatter = true;
			return this;
		}

		/// <summary>
		/// Use this method for custom configuration of the HttpServer.
		/// </summary>
		public HttpServerConfiguration Configure(Action<IOwinConfiguration> configurer)
		{
			if (configurer == null) throw new ArgumentNullException("configurer");

			_configurers.Add(configurer);

			return this;
		}

		internal void Apply(IOwinConfiguration configuration)
		{
			if (configuration == null) throw new ArgumentNullException("configuration");

			Configure(configurer =>
			{
				if (!_allowCaching)
					configurer.Http.MessageHandlers.Add(new NoCachingHandler());
				
				if (!_allowHttpFormatter)
					configurer.Http.Formatters.Remove(configurer.Http.Formatters.XmlFormatter);
			});

			foreach (Action<IOwinConfiguration> configurer in _configurers)
				configurer(configuration);
		}
	}
}