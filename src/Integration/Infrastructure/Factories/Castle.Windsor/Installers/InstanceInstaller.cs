using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	internal class InstanceInstaller<T> : IWindsorInstaller where T : class
    {
        private readonly T _instance;
	    private readonly Action<ComponentRegistration<T>> _registration;

	    public InstanceInstaller(T instance, Action<ComponentRegistration<T>> registration = null)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            _instance = instance;
	        _registration = registration;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            ComponentRegistration<T> registration = Component
                .For<T>()
                .Instance(_instance);

            _registration?.Invoke(registration);

            container.Register(registration);
        }
    }
}