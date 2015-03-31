using System;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Model;

namespace Vertica.Integration
{
    internal static class CastleWindsor
	{
        public static IWindsorContainer Initialize(ApplicationConfiguration configuration)
		{
		    if (configuration == null) throw new ArgumentNullException("configuration");

            IWindsorContainer container = InitializeContainer(configuration);

            container.Kernel.AddFacility<TypedFactoryFacility>();
			container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel, allowEmptyCollections: true));
			container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<LazyOfTComponentLoader>());

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

			return container;
		}

        private static IWindsorContainer InitializeContainer(ApplicationConfiguration configuration)
        {
            string configurationFileName = null;
            configuration.Tasks(x => configurationFileName = x.ConfigurationFileName);

            if (!String.IsNullOrWhiteSpace(configurationFileName))
                return new WindsorContainer(new XmlInterpreter(configurationFileName));

            return new WindsorContainer();
        }
	}
}