using System;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration
{
    internal static class CastleWindsor
	{
        public static IWindsorContainer Initialize(ApplicationConfiguration configuration)
		{
		    if (configuration == null) throw new ArgumentNullException("configuration");

            var container = new WindsorContainer();
            container.Kernel.AddFacility<TypedFactoryFacility>();
            container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<LazyOfTComponentLoader>());

            foreach (IInitializable<IWindsorContainer> subject in configuration.ContainerInitializations)
                subject.Initialize(container);

            container.Install(
                new ConventionInstaller()
                    .AddFromAssemblyOfThis<ConventionInstaller>()
					.Ignore<IHost>()
                    .Ignore<ITask>()
                    .Ignore<IStep>());

            // Note: CollectionResolver is not added as this will cause problems for the StepResolver.

			return container;
		}
	}
}