using System;
using System.Linq;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.Windsor;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Infrastructure.Logging;
using Vertica.Integration.Infrastructure.Parsing;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Hosting;

namespace Vertica.Integration
{
    internal static class CastleWindsor
	{
        public static IWindsorContainer Initialize(ApplicationConfiguration configuration)
		{
	        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var container = new WindsorContainer();
            container.Kernel.AddFacility<TypedFactoryFacility>();
            container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<LazyOfTComponentLoader>());

	        configuration.Extensibility(extensibility =>
	        {
		        foreach (var subject in extensibility.OfType<IInitializable<IWindsorContainer>>())
			        subject.Initialize(container);
	        });

            container.Install(
				Install.ByConvention
                    .AddFromAssemblyOfThis<ConventionInstaller>()
					.Ignore<IApplicationContext>()
					.Ignore<IHost>()
                    .Ignore<ITask>()
                    .Ignore<IStep>()
                    .Ignore<ITarget>()
                    .Ignore<CsvRow.ICsvRowBuilder>());

            // Note: CollectionResolver is not added as this will cause problems for the StepResolver.

			return container;
		}
	}
}