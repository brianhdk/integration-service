using System;
using System.Collections.Generic;
using System.Web.Http;
using Castle.MicroKernel;
using Microsoft.Owin.BuilderProperties;
using Owin;

namespace Vertica.Integration.WebApi.Infrastructure
{
	public class HttpServerConfiguration
	{
		private readonly List<Action<IOwinConfiguration>> _configurers;
		private bool _allowCaching;
		private bool _allowHttpFormatter;
		private IncludeErrorDetailPolicy _errorDetailPolicy;

		internal HttpServerConfiguration()
		{
			_configurers = new List<Action<IOwinConfiguration>>();
			_errorDetailPolicy = IncludeErrorDetailPolicy.Always;
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
		/// By default Integration Service will set the IncludeErrorDetailPolicy to Always. Use this method to override that behaviour.
		/// </summary>
		public HttpServerConfiguration ErrorDetailPolicy(IncludeErrorDetailPolicy policy)
		{
			_errorDetailPolicy = policy;
			return this;
		}

		/// <summary>
		/// Use this method for custom configuration of the HttpServer.
		/// </summary>
		public HttpServerConfiguration Configure(Action<IOwinConfiguration> configurer)
		{
			if (configurer == null) throw new ArgumentNullException(nameof(configurer));

			_configurers.Add(configurer);

			return this;
		}

		internal IDisposable CreateHttpServer(IKernel kernel, string url)
		{
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));

			return new HttpServer(kernel, Apply, url);
		}

	    internal void Configure(IKernel kernel, IAppBuilder app)
	    {
	        if (kernel == null) throw new ArgumentNullException(nameof(kernel));
	        if (app == null) throw new ArgumentNullException(nameof(app));

	        app.Configure(new AppProperties(app.Properties), kernel, Apply);
	    }

		private void Apply(IOwinConfiguration configuration)
		{
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));

			Configure(configurer =>
			{
				configurer.Http.IncludeErrorDetailPolicy = _errorDetailPolicy;

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