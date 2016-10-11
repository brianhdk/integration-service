using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	internal class ServiceInstaller<TService> : IWindsorInstaller where TService : class
	{
		private readonly Action<ComponentRegistration<TService>> _registration;

		public ServiceInstaller(Action<ComponentRegistration<TService>> registration = null)
		{
			_registration = registration;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			ComponentRegistration<TService> registration = Component.For<TService>().ImplementedBy<TService>();

			if (_registration != null)
				_registration(registration);

			container.Register(registration);
		}
	}

	internal class ServiceInstaller<TService, TImplementation> : IWindsorInstaller 
		where TService : class
		where TImplementation : class, TService
	{
		private readonly Action<ComponentRegistration<TService>> _registration;

		public ServiceInstaller(Action<ComponentRegistration<TService>> registration = null)
		{
			_registration = registration;
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			ComponentRegistration<TService> registration = Component
                .For<TService>()
                .ImplementedBy<TImplementation>();

		    _registration?.Invoke(registration);

		    container.Register(registration);
		}
	}
}