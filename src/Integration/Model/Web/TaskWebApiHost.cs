using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace Vertica.Integration.Model.Web
{
    public class TaskWebApiHost : WebApiHost
    {
        private const string ContextKey = "Context_7A89A4949AC44CCB8D7417AA78BB000B";

        private readonly ITask _task;
        private readonly string[] _arguments;
        private readonly IWindsorContainer _container;

        public TaskWebApiHost(string url, ITask task, string[] arguments, TextWriter outputter, IWindsorContainer container)
            : base(url, task, outputter, container)
        {
            _task = task;
            _arguments = arguments;
            _container = container;
        }

        protected override void MapRoutes(HttpConfiguration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            configuration.Routes.MapHttpRoute(
                name: "WebApi",
                routeTemplate: "{controller}",
                defaults: new { controller = "Task" });
        }

        protected override ICollection<Type> GetControllerTypes()
        {
            return new List<Type> { typeof (TaskController) };
        }

        protected override IHttpController CreateController(HttpRequestMessage request, Type controllerType)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (controllerType == null) throw new ArgumentNullException("controllerType");

            if (controllerType != typeof(TaskController))
                throw new InvalidOperationException(
                    String.Format("Controller type '{0}' is not supported.", controllerType.FullName));

            if (!_container.Kernel.HasComponent(typeof(TaskController)))
            {
                _container.Register(
                    Component.For<TaskController>()
                        .LifeStyle.Transient);
            }

            HttpConfiguration configuration = request.GetConfiguration();

            if (configuration != null)
                configuration.Properties[ContextKey] = new Context(_task, _arguments);

            return base.CreateController(request, controllerType);
        }

        internal class Context
        {
            internal Context(ITask task, string[] arguments)
            {
                if (task == null) throw new ArgumentNullException("task");

                Task = task;
                Arguments = arguments;
            }

            public ITask Task { get; private set; }
            public string[] Arguments { get; private set; }

            internal static Context Get(HttpConfiguration configuration)
            {
                if (configuration == null) throw new ArgumentNullException("configuration");

                var context = configuration.Properties[ContextKey] as Context;

                if (context == null)
                    throw new InvalidOperationException("Context not found.");

                return context;
            }
        }
    }
}