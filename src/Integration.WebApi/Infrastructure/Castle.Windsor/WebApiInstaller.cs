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
		    if (scanAssemblies == null) throw new ArgumentNullException(nameof(scanAssemblies));
		    if (addControllers == null) throw new ArgumentNullException(nameof(addControllers));
		    if (removeControllers == null) throw new ArgumentNullException(nameof(removeControllers));
		    if (httpServerConfiguration == null) throw new ArgumentNullException(nameof(httpServerConfiguration));

		    _scanAssemblies = scanAssemblies;
		    _addControllers = addControllers;
		    _removeControllers = removeControllers;
		    _httpServerConfiguration = httpServerConfiguration;
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
		    private readonly HttpServerConfiguration _configuration;

		    public HttpServerFactory(IKernel kernel, HttpServerConfiguration configuration)
		    {
			    _kernel = kernel;
			    _configuration = configuration;
		    }

			public IDisposable Create(string url)
			{
				return _configuration.CreateHttpServer(_kernel, url);
		    }
	    }

	    private class ControllerTypes : IWebApiControllers
        {
		    public ControllerTypes(Type[] types)
            {
                Controllers = types;
            }

            public Type[] Controllers { get; }
        }
	}
}