using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;

namespace Vertica.Integration.WebApi.Infrastructure.Castle.Windsor
{
	internal class WebApiInstaller : IWindsorInstaller
	{
		private readonly HttpServerConfiguration _httpServerConfiguration;
		private readonly Assembly[] _scanAssemblies;
        private readonly Type[] _addControllers;
        private readonly Type[] _removeControllers;

	    public WebApiInstaller(Assembly[] scanAssemblies, Type[] addControllers, Type[] removeControllers, HttpServerConfiguration httpServerConfiguration)
        {
		    _scanAssemblies = scanAssemblies ?? new Assembly[0];
		    _addControllers = addControllers ?? new Type[0];
		    _removeControllers = removeControllers ?? new Type[0];
		    _httpServerConfiguration = httpServerConfiguration ?? new HttpServerConfiguration();
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
			container.Register(
				Component.For<IHttpServerFactory>()
					.UsingFactoryMethod(kernel => new HttpServerFactory(kernel, _httpServerConfiguration)));

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
		    private readonly HttpServerConfiguration _httpServerConfiguration;

		    public HttpServerFactory(IKernel kernel, HttpServerConfiguration httpServerConfiguration)
		    {
			    _kernel = kernel;
			    _httpServerConfiguration = httpServerConfiguration;
		    }

			public IDisposable Create(string url)
		    {
			    return new HttpServer(url, _kernel, configuration => _httpServerConfiguration.Apply(configuration));
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