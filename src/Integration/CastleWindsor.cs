using System;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Model;

namespace Vertica.Integration
{
    internal static class CastleWindsor
	{
        public static IWindsorContainer Initialize(ApplicationConfiguration configuration)
		{
		    if (configuration == null) throw new ArgumentNullException("configuration");

            IWindsorContainer container = new WindsorContainer();

            foreach (IInitializable<IWindsorContainer> subject in configuration.ContainerInitializations)
                subject.Initialize(container);

            container.Install(
                new ConsoleWriterInstaller(),
                new ConventionInstaller()
                    .AddFromAssemblyOfThis<ConventionInstaller>()
                    .Ignore<ITask>()
                    .Ignore<IStep>());

            // Note: CollectionResolver is not added as this will cause problems for the StepResolver.

			return container;
		}
	}
}