using System.Reflection;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.Windsor;
using Castle.Windsor.Configuration.Interpreters;
using Vertica.Integration.Infrastructure.Database.NHibernate.Castle.Windsor;
using Vertica.Integration.Infrastructure.Database.NHibernate.Connections;
using Vertica.Integration.Infrastructure.Factories;
using Vertica.Integration.Infrastructure.Factories.Castle.Windsor.Installers;
using Vertica.Integration.Model;
using Vertica.Integration.Properties;

namespace Vertica.Integration
{
	public static class Bootstrapper
	{
		internal static IWindsorContainer Run()
		{
			IWindsorContainer container = 
				ObjectFactory.Create(() => new WindsorContainer(new XmlInterpreter()));

			container.Kernel.AddFacility<TypedFactoryFacility>();
			container.Kernel.Resolver.AddSubResolver(new CollectionResolver(container.Kernel, allowEmptyCollections: true));
			container.Register(Component.For<ILazyComponentLoader>().ImplementedBy<LazyOfTComponentLoader>());

			container.Register(Component.For<ISettings>().UsingFactoryMethod(x => Settings.Default));

			Assembly integrationAssembly = typeof (Bootstrapper).Assembly;

			container.Install(
				new NHibernateInstaller(new IntegrationDb()),
				new TaskFactoryInstaller(),
                new ConsoleWriterInstaller(),
                new WebApiInstaller(new[] { integrationAssembly }),
				new ConventionInstaller(new[] { integrationAssembly }, typeof(ITask), typeof(IStep), typeof(ISettings)));

			return container;
		}
	}
}