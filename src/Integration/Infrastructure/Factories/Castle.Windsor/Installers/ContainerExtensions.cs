using System;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
    public static class ContainerExtensions
    {
        public static void RegisterInstance<T>(this IWindsorContainer container, T instance) 
            where T : class
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            container.Install(new InstanceInstaller<T>(instance));
        }
    }
}