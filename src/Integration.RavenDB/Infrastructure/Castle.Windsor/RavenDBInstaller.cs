using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.RavenDB.Infrastructure.Castle.Windsor
{
	internal class RavenDbInstaller : RavenDbInstaller<DefaultConnection>
	{
		public RavenDbInstaller(DefaultConnection connection)
			: base(connection)
		{
		}

		public override void Install(IWindsorContainer container, IConfigurationStore store)
		{
			if (container.Kernel.HasComponent(typeof(IRavenDbFactory)))
				throw new InvalidOperationException("Only one DefaultConnection can be installed. Use the generic installer for additional instances.");

			base.Install(container, store);

			container.Register(
				Component.For<IRavenDbFactory>()
					.UsingFactoryMethod(kernel => 
						new RavenDbFactory(kernel.Resolve<IRavenDbFactory<DefaultConnection>>())));
		}
	}

	internal class RavenDbInstaller<TConnection> : IWindsorInstaller
		where TConnection : Connection
	{
		private readonly TConnection _connection;

		public RavenDbInstaller(TConnection connection)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			_connection = connection;
		}

		public virtual void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(
				Component.For<IRavenDbFactory<TConnection>>()
					.UsingFactoryMethod(() => new RavenDbFactory<TConnection>(_connection)));
		}
	}
}