using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	internal class ServiceInstaller<TService> : ServiceInstaller<TService, TService>
        where TService : class
	{
	    public ServiceInstaller(Action<ComponentRegistration<TService>> registration = null)
            : base(registration)
	    {
	    }
	}

	internal class ServiceInstaller<TService, TImplementation> : IWindsorInstaller 
		where TService : class
		where TImplementation : class, TService
	{
		private readonly Action<ComponentRegistration<TService>> _registration;

		public ServiceInstaller(Action<ComponentRegistration<TService>> registration = null)
		{
		    _registration = registration ?? (x => x.LifestyleSingleton());
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			ComponentRegistration<TService> registration = Component
                .For<TService>()
                .ImplementedBy<TImplementation>();

		    _registration.Invoke(registration);

		    container.Register(registration);
		}
	}
}