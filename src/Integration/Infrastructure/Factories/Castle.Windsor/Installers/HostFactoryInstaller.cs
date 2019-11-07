using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers
{
    internal class HostFactoryInstaller : IWindsorInstaller
	{
		public void Install(IWindsorContainer container, IConfigurationStore store)
		{
		    container.Register(
		        Component.For<IHostFactory>()
		            .UsingFactoryMethod((kernel, model, context) => new HostFactory(kernel))
                    .LifestyleSingleton());
		}

        private class HostFactory : IHostFactory
        {
            private readonly IKernel _kernel;

            public HostFactory(IKernel kernel)
            {
                _kernel = kernel;
            }

            public IHost[] GetAll()
            {
                return _kernel.ResolveAll<IHost>();
            }
        }
	}
}