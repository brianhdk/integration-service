using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.RavenDB.Infrastructure.Castle.Windsor
{
	internal class RavenDBInstaller : RavenDBInstaller<DefaultConnection>
	{
		public RavenDBInstaller(DefaultConnection connection)
			: base(connection)
		{
		}

		public override void Install(IWindsorContainer container, IConfigurationStore store)
		{
			if (container.Kernel.HasComponent(typeof(IRavenDBFactory)))
				throw new InvalidOperationException("Only one DefaultConnection can be installed. Use the generic installer for additional instances.");

			base.Install(container, store);

			container.Register(
				Component.For<IRavenDBFactory>()
					.UsingFactoryMethod(kernel => 
						new RavenDBFactory(kernel.Resolve<IRavenDBFactory<DefaultConnection>>())));
		}
	}

	internal class RavenDBInstaller<TConnection> : IWindsorInstaller
		where TConnection : Connection
	{
		private readonly TConnection _connection;

		public RavenDBInstaller(TConnection connection)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			_connection = connection;
		}

		public virtual void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(
				Component.For<IRavenDBFactory<TConnection>>()
					.UsingFactoryMethod(() => new RavenDBFactory<TConnection>(_connection)));
		}
	}
}