using System;
using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
    public static class ContainerExtensions
    {
        [Obsolete("Use Services(services => services.Advanced(advanced => advanced.Register(instance)))")]
        public static void RegisterInstance<T>(this IWindsorContainer container, T instance, Action<ComponentRegistration<T>> registration = null) 
            where T : class
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            container.Install(new InstanceInstaller<T>(instance, registration));
        }
    }
}