using System;
using System.Reflection;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Vertica.Integration.Infrastructure.Database.Dapper.Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.Dapper.Databases;
using Vertica.Integration.Infrastructure.Database.Migrations;
using Vertica.Integration.Infrastructure.Factories;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Model;
using Vertica.Integration.Model.Web;
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

			Assembly integrationAssembly = typeof(CastleWindsor).Assembly;

            AutoRegistredTasksConfiguration autoRegistredTasks = null;
            configuration.AutoRegistredTasks(x => autoRegistredTasks = x);

            WebApiConfiguration webApi = null;
            configuration.WebApi(x => webApi = x);

			container.Install(
                new DapperInstaller(new IntegrationDb(configuration.DatabaseConnectionStringName)),
                new AutoRegisterTasksInstaller(autoRegistredTasks),
				new TaskFactoryInstaller(),
                new ConsoleWriterInstaller(),
                new WebApiInstaller(webApi),
				new ConventionInstaller(new[] { integrationAssembly }, typeof(ITask), typeof(IStep), typeof(ISettings)));

            container.Install(configuration.CustomInstallers);

            MigrationConfiguration migration = null;
            configuration.Migration(x => migration = x.Lock());

            container.Register(
                Component.For<MigrationConfiguration>()
                    .UsingFactoryMethod(() => migration));

			return container;
		}
	}
}