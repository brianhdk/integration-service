using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
    public class SingletonInstanceInstaller<T> : IWindsorInstaller where T : class
    {
        private readonly T _instance;

        public SingletonInstanceInstaller(T instance)
        {
            if (instance == null) throw new ArgumentNullException("instance");

            _instance = instance;
        }

        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(
                Component.For<T>()
                    .UsingFactoryMethod(() => _instance));
        }
    }
}