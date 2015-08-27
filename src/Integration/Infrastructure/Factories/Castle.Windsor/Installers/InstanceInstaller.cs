using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
	internal class InstanceInstaller<T> : IWindsorInstaller where T : class
    {
        private readonly T _instance;

        public InstanceInstaller(T instance)
        {
            if (instance == null) throw new ArgumentNullException("instance");

            _instance = instance;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<T>().Instance(_instance));
        }
    }
}