using System;
using Castle.Facilities.TypedFactory;
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
            container.Kernel.AddFacility<TypedFactoryFacility>();

            configuration.Tasks(x => x.Install(container));
            configuration.WebApi(x => x.Install(container));
            configuration.Migration(x => x.Install(container));

            container.Install(
                new ConsoleWriterInstaller(),
                new ConventionInstaller()
                    .AddFromAssemblyOfThis<ITask>()
                    .Ignore<ITask>()
                    .Ignore<IStep>());

            container.Install(configuration.CustomInstallers);

            // Note: CollectionResolver is not added as this will cause problems for the StepResolver.

			return container;
		}
	}
}