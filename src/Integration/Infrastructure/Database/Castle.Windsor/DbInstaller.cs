using System;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Database.Castle.Windsor
{
    internal class DbInstaller : DbInstaller<DefaultConnection>
	{
		public DbInstaller(DefaultConnection connection)
			: base(connection)
		{
		}

		public override void Install(IWindsorContainer container, IConfigurationStore store)
		{
            if (container.Kernel.HasComponent(typeof(IDbFactory)))
                throw new InvalidOperationException("Only one DefaultConnection can be installed. Use the generic installer for additional instances.");

			base.Install(container, store);

		    container.Register(
		        Component.For<IDbFactory>()
		            .UsingFactoryMethod(x => new DbFactory(x.Resolve<IDbFactory<DefaultConnection>>())));
		}
	}

	public class DbInstaller<TConnection> : IWindsorInstaller
		where TConnection : Connection
	{
		private readonly TConnection _connection;

	    public DbInstaller(TConnection connection)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			_connection = connection;
		}

		public virtual void Install(IWindsorContainer container, IConfigurationStore store)
		{
		    container.Register(
		        Component.For<IDbFactory<TConnection>>()
		            .UsingFactoryMethod(() => new DbFactory<TConnection>(Connection.ConnectionString)));
		}

		protected TConnection Connection
		{
			get { return _connection; }
		}
	}
}