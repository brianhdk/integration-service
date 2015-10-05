using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using Castle.MicroKernel;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Owin;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.WebApi.Infrastructure.Castle.Windsor;

namespace Vertica.Integration.WebApi.Infrastructure
{
	internal class HttpServer : IDisposable
    {
        private readonly IDisposable _httpServer;
        private readonly IKernel _kernel;
		private readonly TextWriter _outputter;

		public HttpServer(string url, IKernel kernel, Action<IOwinConfiguration> configuration = null)
        {
	        if (String.IsNullOrWhiteSpace(url)) throw new ArgumentException(@"Value cannot be null or empty.", "url");
	        if (kernel == null) throw new ArgumentNullException("kernel");

	        _kernel = kernel;
	        _outputter = kernel.Resolve<TextWriter>();

			_outputter.WriteLine("Starting HttpServer listening on URL: {0}", url);
			_outputter.WriteLine();

			// TODO: Make it possible to add multiple URL's to listen on
	        _httpServer = WebApp.Start(new StartOptions(url), builder =>
            {
				builder.Properties["host.TraceOutput"] = _outputter;

                var httpConfiguration = new HttpConfiguration();

				if (configuration != null)
					configuration(new OwinConfiguration(builder, httpConfiguration, kernel));

				httpConfiguration.MessageHandlers.Add(new NoCachingHandler());
				httpConfiguration.Formatters.Remove(httpConfiguration.Formatters.XmlFormatter);

				ConfigureJson(httpConfiguration);
				ConfigureServices(httpConfiguration);

				MapRoutes(httpConfiguration);

				builder.UseWebApi(httpConfiguration);
            });
        }

        private void ConfigureServices(HttpConfiguration configuration)
        {
            var resolver = new CustomResolver(GetControllerTypes, CreateController);

            configuration.Services.Replace(typeof (IAssembliesResolver), resolver);
            configuration.Services.Replace(typeof (IHttpControllerTypeResolver), resolver);
            configuration.Services.Replace(typeof (IHttpControllerActivator), resolver);

			configuration.Services.Add(typeof(IExceptionLogger), new ExceptionLogger(_kernel.Resolve<ILogger>()));
        }

        private ICollection<Type> GetControllerTypes()
        {
            return _kernel.Resolve<IWebApiControllers>().Controllers;
        }

        private IHttpController CreateController(HttpRequestMessage request, Type controllerType)
        {
            return _kernel.Resolve(controllerType) as IHttpController;
        }

        private void MapRoutes(HttpConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            configuration.MapHttpAttributeRoutes();

	        if (!configuration.Routes.ContainsKey("WebApi"))
	        {
				configuration.Routes.MapHttpRoute(
					name: "WebApi",
					routeTemplate: "{controller}",
					defaults: new { controller = "Home" });
	        }
        }

        private static void ConfigureJson(HttpConfiguration configuration)
        {
            JsonSerializerSettings jsonSettings =
                configuration.Formatters.JsonFormatter.SerializerSettings;

            jsonSettings.Formatting = Formatting.Indented;
            jsonSettings.Converters.Add(new StringEnumConverter());
        }

        public void Dispose()
        {
            if (_httpServer != null)
            {
	            _outputter.WriteLine("Shutting down HttpServer.");
				_outputter.WriteLine();

	            _httpServer.Dispose();
            }
        }

		private class OwinConfiguration : IOwinConfiguration
		{
			internal OwinConfiguration(IAppBuilder app, HttpConfiguration httpConfiguration, IKernel kernel)
			{
				if (app == null) throw new ArgumentNullException("app");
				if (httpConfiguration == null) throw new ArgumentNullException("httpConfiguration");
				if (kernel == null) throw new ArgumentNullException("kernel");

				App = app;
				Http = httpConfiguration;
				Kernel = kernel;
			}

			public IAppBuilder App { get; private set; }
			public HttpConfiguration Http { get; private set; }
			public IKernel Kernel { get; private set; }
		}

        private class CustomResolver : IAssembliesResolver, IHttpControllerTypeResolver, IHttpControllerActivator
        {
            private readonly Func<ICollection<Type>> _controllerTypes;
            private readonly Func<HttpRequestMessage, Type, IHttpController> _createController;

            public CustomResolver(Func<ICollection<Type>> controllerTypes, Func<HttpRequestMessage, Type, IHttpController> createController)
            {
                if (controllerTypes == null) throw new ArgumentNullException("controllerTypes");
                if (createController == null) throw new ArgumentNullException("createController");

                _controllerTypes = controllerTypes;
                _createController = createController;
            }

            public ICollection<Assembly> GetAssemblies()
            {
                return new List<Assembly>();
            }

            public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
            {
                return _controllerTypes();
            }

            public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
            {
                return _createController(request, controllerType);
            }
        }
    }
}