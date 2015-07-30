using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Model.Web;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
    internal class WebApiInstaller : IWindsorInstaller
	{
        private readonly Assembly[] _scanAssemblies;
        private readonly Type[] _addControllers;
        private readonly Type[] _removeControllers;

        public WebApiInstaller(Assembly[] scanAssemblies, Type[] addControllers, Type[] removeControllers)
        {
            _scanAssemblies = scanAssemblies ?? new Assembly[0];
            _addControllers = addControllers ?? new Type[0];
            _removeControllers = removeControllers ?? new Type[0];
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
			container.Register(
				Component.For<IHttpServerFactory>()
					.UsingFactoryMethod(kernel => new HttpServerFactory(kernel)));

            var types = new List<Type>();

            foreach (Assembly assembly in _scanAssemblies.Distinct())
            {
                container.Register(
                    Classes.FromAssembly(assembly)
                        .BasedOn<ApiController>()
                        .Unless(x =>
                        {
                            if (_removeControllers.Contains(x))
                                return true;

                            types.Add(x);
                            return false;
                        })
                        .WithServiceSelf()
                        .LifestyleTransient());
            }

            container.Register(
                Classes.From(_addControllers.Except(_removeControllers).Distinct())
                    .BasedOn<ApiController>()
                    .Expose(types.Add)
                    .WithServiceSelf()
                    .LifestyleTransient());

            container.RegisterInstance<IWebApiControllers>(new ControllerTypes(types.ToArray()));
        }

	    private class HttpServerFactory : IHttpServerFactory
	    {
		    private readonly IKernel _kernel;

		    public HttpServerFactory(IKernel kernel)
		    {
			    _kernel = kernel;
		    }

		    public IDisposable Create(string url)
		    {
			    return new Model.Web.HttpServer(url, _kernel);
		    }
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