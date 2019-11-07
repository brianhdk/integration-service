using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
    internal class TypeInstaller : IWindsorInstaller
    {
        private readonly Type _serviceType;
        private readonly Type _implementationType;
        private readonly Action<ComponentRegistration<object>> _registration;

        public TypeInstaller(Type serviceType, Type implementationType, Action<ComponentRegistration<object>> registration = null)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));

            _serviceType = serviceType;
            _implementationType = implementationType;
            _registration = registration ?? (x => x.LifestyleSingleton());
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            ComponentRegistration<object> registration = Component.For(_serviceType).ImplementedBy(_implementationType);

            _registration.Invoke(registration);

            container.Register(registration);
        }
    }

	internal class TypeInstaller<TService> : IWindsorInstaller
        where TService : class
	{
		private readonly Type _implementationType;
		private readonly Action<ComponentRegistration<TService>> _registration;

		public TypeInstaller(Type implementationType, Action<ComponentRegistration<TService>> registration = null)
		{
			if (implementationType == null) throw new ArgumentNullException(nameof(implementationType));

			_implementationType = implementationType;
			_registration = registration ?? (x => x.LifestyleSingleton());
		}

		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
			ComponentRegistration<TService> registration = Component.For<TService>().ImplementedBy(_implementationType);

			_registration.Invoke(registration);

			container.Register(registration);
		}
	}
}