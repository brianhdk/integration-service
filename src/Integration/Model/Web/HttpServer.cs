using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Castle.MicroKernel;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Owin;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model.Web
{
	internal class HttpServer : IDisposable
    {
        private readonly IDisposable _httpServer;
        private readonly IKernel _kernel;

        public HttpServer(string url, IKernel kernel)
        {
	        if (String.IsNullOrWhiteSpace(url)) throw new ArgumentException(@"Value cannot be null or empty.", "url");
	        if (kernel == null) throw new ArgumentNullException("kernel");

	        _kernel = kernel;

	        _httpServer = WebApp.Start(new StartOptions(url), builder =>
            {
                var configuration = new HttpConfiguration();

                configuration.Filters.Add(new ExceptionHandlingAttribute(_kernel.Resolve<ILogger>()));
                configuration.MessageHandlers.Add(new CachingHandler());
                configuration.Formatters.Remove(configuration.Formatters.XmlFormatter);

                ConfigureJson(configuration);
                MapRoutes(configuration);
                ConfigureServices(configuration);

                builder.UseWebApi(configuration);
            });
        }

        private void ConfigureServices(HttpConfiguration configuration)
        {
            var resolver = new CustomResolver(GetControllerTypes, CreateController);

            configuration.Services.Replace(typeof (IAssembliesResolver), resolver);
            configuration.Services.Replace(typeof (IHttpControllerTypeResolver), resolver);
            configuration.Services.Replace(typeof (IHttpControllerActivator), resolver);
        }

        protected virtual ICollection<Type> GetControllerTypes()
        {
            return _kernel.Resolve<IWebApiControllers>().Controllers;
        }

        protected virtual IHttpController CreateController(HttpRequestMessage request, Type controllerType)
        {
            return _kernel.Resolve(controllerType) as IHttpController;
        }

        protected virtual void MapRoutes(HttpConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            configuration.MapHttpAttributeRoutes();

            configuration.Routes.MapHttpRoute(
                name: "WebApi",
                routeTemplate: "{controller}",
                defaults: new {controller = "Home"});
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
                _httpServer.Dispose();
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