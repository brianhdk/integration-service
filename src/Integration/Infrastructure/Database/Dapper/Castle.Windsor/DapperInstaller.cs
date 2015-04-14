using System;
using System.Data;
using System.Data.SqlClient;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Database.Dapper.Castle.Windsor
{
	public class DapperInstaller : DapperInstaller<DefaultConnection>
	{
		public DapperInstaller(DefaultConnection connection)
			: base(connection)
		{
		}

		public override void Install(IWindsorContainer container, IConfigurationStore store)
		{
            if (container.Kernel.HasComponent(typeof(IDapperFactory)))
                throw new InvalidOperationException("Only one DefaultConnection can be installed. Use the generic installer for additional instances.");

			base.Install(container, store);

			container.Register(
                Component.For<IDapperFactory>()
				    .AsFactory(cfg => cfg.SelectedWith(Connection.SelectorName)));
		}
	}

	public class DapperInstaller<TConnection> : IWindsorInstaller
		where TConnection : Connection
	{
		private readonly TConnection _connection;

	    public DapperInstaller(TConnection connection)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			_connection = connection;
		}

		public virtual void Install(IWindsorContainer container, IConfigurationStore store)
		{
		    container.Register(
		        Component.For<IDbConnection>()
		            .Named(_connection.DbConnectionName)
		            .UsingFactoryMethod(kernel => new SqlConnection(_connection.ConnectionStringInternal))
                    .LifeStyle.Transient);

		    container.Register(
		        Component.For<IDapperSession>()
		            .Named(_connection.SessionName)
		            .UsingFactoryMethod(kernel =>
		                new DapperSession(
		                    kernel.Resolve<IDbConnection>(_connection.DbConnectionName)))
                    .LifeStyle.Transient);

		    container.Register(
		        Component.For<DapperSelector<TConnection>>()
		            .Named(_connection.SelectorName)
		            .UsingFactoryMethod(() => new DapperSelector<TConnection>(_connection)));

		    container.Register(
		        Component.For<IDapperFactory<TConnection>>()
		            .Named(_connection.FactoryName)
		            .AsFactory(cfg => cfg.SelectedWith(_connection.SelectorName)));
		}

		protected TConnection Connection
		{
			get { return _connection; }
		}
	}
}