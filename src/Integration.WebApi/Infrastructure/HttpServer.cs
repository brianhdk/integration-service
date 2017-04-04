using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.Http.Dependencies;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Owin;
using Vertica.Integration.Infrastructure;
using Vertica.Integration.Infrastructure.IO;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.WebApi.Infrastructure.Castle.Windsor;
using IDependencyResolver = System.Web.Http.Dependencies.IDependencyResolver;

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

			throw new ArgumentOutOfRangeException(nameof(url), url, $@"'{url}' is not a valid absolute url.");
		}

		private void ConfigureServices(HttpConfiguration configuration)
        {
            var resolver = new CustomResolver(GetControllerTypes);

            configuration.Services.Replace(typeof (IAssembliesResolver), resolver);
            configuration.Services.Replace(typeof (IHttpControllerTypeResolver), resolver);

			configuration.Services.Add(typeof(IExceptionLogger), new ExceptionLogger(_kernel.Resolve<ILogger>()));

            configuration.DependencyResolver = new CustomDependencyResolver(_kernel, configuration.DependencyResolver);
        }

        private ICollection<Type> GetControllerTypes()
        {
            return _kernel.Resolve<IWebApiControllers>().Controllers;
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

        private class CustomResolver : IAssembliesResolver, IHttpControllerTypeResolver
        {
            private readonly Func<ICollection<Type>> _controllerTypes;

            public CustomResolver(Func<ICollection<Type>> controllerTypes)
            {
                _controllerTypes = controllerTypes;
            }

            public ICollection<Assembly> GetAssemblies()
            {
                return new List<Assembly>();
            }

            public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
            {
                return _controllerTypes();
            }
        }

        private class CustomDependencyResolver : IDependencyResolver
        {
            private readonly IKernel _kernel;
            private readonly IDependencyResolver _currentResolver;

            public CustomDependencyResolver(IKernel kernel, IDependencyResolver currentResolver)
            {
                _kernel = kernel;
                _currentResolver = currentResolver;
            }

            public void Dispose()
            {
                _currentResolver.Dispose();
            }

            public IDependencyScope BeginScope()
            {
                return new CustomDependencyScope(_kernel, _currentResolver);
            }

            public object GetService(Type serviceType)
            {
                bool hasComponent = _kernel.HasComponent(serviceType);

                return hasComponent
                    ? _kernel.Resolve(serviceType)
                    : _currentResolver.GetService(serviceType);
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                bool hasComponent = _kernel.HasComponent(serviceType);

                return hasComponent
                    ? _kernel.ResolveAll(serviceType).OfType<object>().ToArray()
                    : _currentResolver.GetServices(serviceType);
            }
        }

        internal sealed class CustomDependencyScope : IDependencyScope
        {
            private readonly IKernel _kernel;

            private readonly IDisposable _windsorScope;
            private readonly IDependencyScope _currentResolverScope;

            public CustomDependencyScope(IKernel kernel, IDependencyResolver currentResolver)
            {
                if (kernel == null) throw new ArgumentNullException(nameof(kernel));
                if (currentResolver == null) throw new ArgumentNullException(nameof(currentResolver));

                _kernel = kernel;

                _windsorScope = kernel.BeginScope();
                _currentResolverScope = currentResolver.BeginScope();
            }

            public void Dispose()
            {
                _currentResolverScope.Dispose();
                _windsorScope.Dispose();
            }

            public object GetService(Type serviceType)
            {
                bool hasComponent = _kernel.HasComponent(serviceType);

                return hasComponent
                    ? _kernel.Resolve(serviceType)
                    : _currentResolverScope.GetService(serviceType);
            }

            public IEnumerable<object> GetServices(Type serviceType)
            {
                bool hasComponent = _kernel.HasComponent(serviceType);

                return hasComponent
                    ? _kernel.ResolveAll(serviceType).OfType<object>()
                    : _currentResolverScope.GetServices(serviceType);
            }
        }
    }
}