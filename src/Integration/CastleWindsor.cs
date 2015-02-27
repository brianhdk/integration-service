using System;
using System.Reflection;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Vertica.Integration.Infrastructure.Database.Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.Dapper.Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.NHibernate.Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.NHibernate.Connections;
using Vertica.Integration.Infrastructure.Factories;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Model;
using Vertica.Integration.Properties;

namespace Vertica.Integration
{
    internal static class CastleWindsor
	{
        public static IWindsorContainer Initialize(ApplicationConfiguration configuration)
		{
		    if (configuration == null) throw new ArgumentNullException("configuration");

		    IWindsorContainer container = 
				ObjectFactory.Create(() => new WindsorContainer(new XmlInterpreter()));

			container.Kernel.AddFacility<TypedFactoryFacility>();
			container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel, allowEmptyCollections: true));
			container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<LazyOfTComponentLoader>());

			container.Register(Component.For<ISettings>().UsingFactoryMethod(x => Settings.Default));

			Assembly integrationAssembly = typeof (CastleWindsor).Assembly;

            WebApiConfiguration webApi = null;
            configuration.WebApi(x => webApi = x.ScanAssembly(integrationAssembly));

			container.Install(
                new DapperInstaller(new Infrastructure.Database.Dapper.Databases.IntegrationDb(configuration.DatabaseConnectionStringName)),
				new NHibernateInstaller(new IntegrationDb(configuration.DatabaseConnectionStringName)),
                new DbInstaller(),
				new TaskFactoryInstaller(),
                new ConsoleWriterInstaller(),
                new WebApiInstaller(webApi),
				new ConventionInstaller(new[] { integrationAssembly }, typeof(ITask), typeof(IStep), typeof(ISettings)));

            container.Install(configuration.CustomInstallers);

			return container;
		}
	}
}