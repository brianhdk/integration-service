using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using Castle.MicroKernel;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Owin;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.WebApi.Infrastructure.Castle.Windsor;

namespace Vertica.Integration.WebApi.Infrastructure
{
	internal class HttpServer : IDisposable
    {
	    private readonly IKernel _kernel;
	    private readonly IConsoleWriter _console;
	    private readonly IDisposable _httpServer;

	    public HttpServer(IKernel kernel, Action<IOwinConfiguration> configuration, string url)
        {
	        if (kernel == null) throw new ArgumentNullException(nameof(kernel));
			if (configuration == null) throw new ArgumentNullException(nameof(configuration));
			AssertUrl(url);

	        _kernel = kernel;
	        _console = kernel.Resolve<IConsoleWriter>();

            Output($"Starting HttpServer listening on URL: {url}");

			// TODO: Make it possible to add multiple URL's to listen on
	        _httpServer = WebApp.Start(new StartOptions(url), builder =>
            {
                // TODO: Look into what this does exactly
				builder.Properties["host.TraceOutput"] = _kernel.Resolve<TextWriter>();

	            HttpConfiguration httpConfiguration = new HttpConfiguration();
				configuration(new OwinConfiguration(builder, httpConfiguration, kernel));

				ConfigureJson(httpConfiguration);
				ConfigureServices(httpConfiguration);

				MapRoutes(httpConfiguration);

                var properties = httpConfiguration.Properties;
                properties["host.OnAppDisposing"] = kernel.Resolve<IShutdown>().Token;
                
                builder.UseWebApi(httpConfiguration);
            });
        }

		private static void AssertUrl(string url)
		{
			Uri dummy;
			if (Uri.TryCreate(url, UriKind.Absolute, out dummy))
				return;

			if (Regex.IsMatch(url ?? string.Empty, @"^http(?:s)?://\+(?:\:\d+)?/?$", RegexOptions.IgnoreCase))
				return;

			throw new ArgumentOutOfRangeException(nameof(url), url, $"'{url}' is not a valid absolute url.");
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
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

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
            JsonSerializerSettings json =
                configuration.Formatters.JsonFormatter.SerializerSettings;

            json.Formatting = Formatting.Indented;
            json.Converters.Add(new StringEnumConverter());
        }

        public void Dispose()
        {
            if (_httpServer != null)
            {
	            Output("Stopping");

	            _httpServer.Dispose();

                Output("Stopped");
            }
        }
        
        private void Output(string message)
        {
            _console.WriteLine($"[HttpServer]: {message}.");
        }

        private class OwinConfiguration : IOwinConfiguration
		{
			internal OwinConfiguration(IAppBuilder app, HttpConfiguration httpConfiguration, IKernel kernel)
			{
				if (app == null) throw new ArgumentNullException(nameof(app));
				if (httpConfiguration == null) throw new ArgumentNullException(nameof(httpConfiguration));
				if (kernel == null) throw new ArgumentNullException(nameof(kernel));

				App = app;
				Http = httpConfiguration;
				Kernel = kernel;
			}

			public IAppBuilder App { get; }
			public HttpConfiguration Http { get; }
			public IKernel Kernel { get; }
		}

        private class CustomResolver : IAssembliesResolver, IHttpControllerTypeResolver, IHttpControllerActivator
        {
            private readonly Func<ICollection<Type>> _controllerTypes;
            private readonly Func<HttpRequestMessage, Type, IHttpController> _createController;

            public CustomResolver(Func<ICollection<Type>> controllerTypes, Func<HttpRequestMessage, Type, IHttpController> createController)
            {
                if (controllerTypes == null) throw new ArgumentNullException(nameof(controllerTypes));
                if (createController == null) throw new ArgumentNullException(nameof(createController));

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