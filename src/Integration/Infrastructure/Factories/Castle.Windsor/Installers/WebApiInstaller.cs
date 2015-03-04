using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Web;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
    internal class WebApiInstaller : IWindsorInstaller
	{
        private readonly WebApiConfiguration _configuration;

        public WebApiInstaller(WebApiConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container
                .Register(Component.For<ITask>()
                .ImplementedBy<WebApiTask>().Named("WebApiTask"));

            var types = new List<Type>();

            foreach (Assembly assembly in _configuration.ScanAssemblies)
            {
                container.Register(
                    Classes.FromAssembly(assembly)
                        .BasedOn<ApiController>()
                        .If(x =>
                        {
                            if (!_configuration.RemoveControllers.Contains(x))
                            {
                                types.Add(x);
                                return true;
                            }

                            return false;
                        })
                        .WithServiceSelf()
                        .LifestyleTransient());
            }

            container.Register(
                Classes.From(_configuration.AddControllers)
                    .BasedOn<ApiController>()
                    .If(x =>
                    {
                        types.Add(x);
                        return true;
                    })
                    .WithServiceSelf()
                    .LifestyleTransient());

            container.Register(
                Component.For<IWebApiControllers>()
                    .UsingFactoryMethod(x => new ControllerTypes(types.ToArray()))
                    .LifeStyle.Singleton);
        }

        private class ControllerTypes : IWebApiControllers
        {
            private readonly Type[] _types;

            public ControllerTypes(Type[] types)
            {
                _types = types;
            }

            public Type[] Controllers
            {
                get { return _types; }
            }
        }
	}
}