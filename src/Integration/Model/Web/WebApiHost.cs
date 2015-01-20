using System;
using System.IO;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using Castle.Windsor;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Owin;
using Vertica.Integration.Infrastructure.Factories;
using Vertica.Integration.Infrastructure.Logging;

namespace Vertica.Integration.Model.Web
{
    public class WebApiHost : IDisposable
    {
        private const string ContextKey = "Context_7A89A4949AC44CCB8D7417AA78BB000B";

        private readonly TaskLog _taskLog;
        private readonly IDisposable _httpServer;

        public WebApiHost(string url, TextWriter outputter, ILogger logger, string taskName, ITask task, params string[] arguments)
        {
            if (String.IsNullOrWhiteSpace(url)) throw new ArgumentException(@"Value cannot be null or empty.", "url");
            if (outputter == null) throw new ArgumentNullException("outputter");
            if (logger == null) throw new ArgumentNullException("logger");
            if (task == null) throw new ArgumentNullException("task");

            _taskLog = new TaskLog(taskName, logger.LogEntry, new Output(outputter.WriteLine));
            _taskLog.LogMessage(String.Format("Starting web-service listening on URL: {0}", url));

            _httpServer = WebApp.Start(new StartOptions(url), builder =>
            {
                var configuration = new HttpConfiguration();

                configuration.Formatters.Remove(configuration.Formatters.XmlFormatter);

                JsonSerializerSettings jsonSettings = 
                    configuration.Formatters.JsonFormatter.SerializerSettings;

                jsonSettings.Formatting = Formatting.Indented;
                jsonSettings.Converters.Add(new StringEnumConverter());

                configuration.MapHttpAttributeRoutes();

                configuration.Routes.MapHttpRoute(
                    name: "WebApi",
                    routeTemplate: "{controller}",
                    defaults: new { controller = "Home" });

                configuration.Filters.Add(new ExceptionHandlingAttribute(logger));

                configuration.Services.Replace(typeof(IHttpControllerActivator), new CustomResolver(ObjectFactory.Instance));

                builder.UseWebApi(configuration);

                configuration.Properties[ContextKey] = new Context(taskName, task, arguments);
            });
        }

        public void Dispose()
        {
            _taskLog.LogMessage("Stopping web-service.");
            _taskLog.Dispose();

            if (_httpServer != null)
                _httpServer.Dispose();
        }

        internal class Context
        {
            internal Context(string taskName, ITask task, string[] arguments)
            {
                if (task == null) throw new ArgumentNullException("task");

                TaskName = taskName;
                Task = task;
                Arguments = arguments;
            }

            public string TaskName { get; private set; }
            public ITask Task { get; private set; }
            public string[] Arguments { get; private set; }

            public static Context Get(HttpConfiguration configuration)
            {
                if (configuration == null) throw new ArgumentNullException("configuration");

                var context = configuration.Properties[ContextKey] as Context;

                if (context == null)
                    throw new InvalidOperationException("Context not found.");

                return context;
            }
        }

        private class CustomResolver : IHttpControllerActivator
        {
            private readonly IWindsorContainer _container;

            public CustomResolver(IWindsorContainer container)
            {
                if (container == null) throw new ArgumentNullException("container");

                _container = container;
            }

            public IHttpController Create(HttpRequestMessage request, HttpControllerDescriptor controllerDescriptor, Type controllerType)
            {
                return _container.Resolve(controllerType) as IHttpController;
            }
        }
    }
}