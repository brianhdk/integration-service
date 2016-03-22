using System;
using System.Collections.Generic;
using System.Web.Http;
using Castle.MicroKernel;

namespace Vertica.Integration.WebApi.Infrastructure
{
	public class HttpServerConfiguration
	{
		private Func<IKernel, HttpConfiguration> _httpConfigurationFactory; 
		private readonly List<Action<IOwinConfiguration>> _configurers;
		private bool _allowCaching;
		private bool _allowHttpFormatter;

		internal HttpServerConfiguration()
		{
			_configurers = new List<Action<IOwinConfiguration>>();
		}

		/// <summary>
		/// Use this method to control how the HttpConfiguration object gets instantiated.
		/// </summary>
		/// <param name="httpConfiguration"></param>
		/// <returns></returns>
		public HttpServerConfiguration CustomHttpConfiguration(Func<IKernel, HttpConfiguration> httpConfiguration)
		{
			if (httpConfiguration == null) throw new ArgumentNullException(nameof(httpConfiguration));

			_httpConfigurationFactory = httpConfiguration;

			return this;
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
			if (configurer == null) throw new ArgumentNullException(nameof(configurer));

			_configurers.Add(configurer);

			return this;
		}

		internal IDisposable CreateHttpServer(IKernel kernel, string url)
		{
			if (kernel == null) throw new ArgumentNullException(nameof(kernel));

			return new HttpServer(url, kernel, HttpConfigurationFactory, Apply);
		}

		private Func<IKernel, HttpConfiguration> HttpConfigurationFactory
		{
			get { return _httpConfigurationFactory ?? (kernel => new HttpConfiguration());}
		}

		private void Apply(IOwinConfiguration configuration)
		{
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));

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