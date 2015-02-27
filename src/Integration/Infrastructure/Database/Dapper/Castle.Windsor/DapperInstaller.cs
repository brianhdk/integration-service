using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Hosting;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Vertica.Integration.Infrastructure.Database.Dapper.Castle.Windsor
{
	public class DapperInstaller : DapperInstaller<DefaultConnection>
	{
        private static readonly Lazy<Action> EnsureSingleton = new Lazy<Action>(() => () => { });
 
		public DapperInstaller(DefaultConnection connection)
			: base(connection)
		{
            if (EnsureSingleton.IsValueCreated)
                throw new InvalidOperationException("Only one DefaultConnection can be installed. Use the generic installer for additional instances.");

            EnsureSingleton.Value();
		}

		public override void Install(IWindsorContainer container, IConfigurationStore store)
		{
			base.Install(container, store);

			container.Register(
                Component.For<IDapperProvider>()
				         .AsFactory(cfg => cfg.SelectedWith(Connection.SelectorName)));
		}
	}

	public class DapperInstaller<TConnection> : IWindsorInstaller
		where TConnection : Connection
	{
		private readonly TConnection _connection;
	    private readonly string _connectionString;

	    private bool _unhandledException;

	    public DapperInstaller(TConnection connection)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			_connection = connection;

            ConnectionStringSettings connectionString =
                ConfigurationManager.ConnectionStrings[_connection.ConnectionStringName];

	        if (connectionString == null)
	            throw new ArgumentException(
                    String.Format("No ConnectionString found with name '{0}'.", _connection.ConnectionStringName));

	        _connectionString = connectionString.ConnectionString;

			if (!HostingEnvironment.IsHosted)
				AppDomain.CurrentDomain.UnhandledException += (obj, ex) => _unhandledException = true;
		}

		public virtual void Install(IWindsorContainer container, IConfigurationStore store)
		{
		    container.Register(
		        Component.For<IDbConnection>()
		            .Named(_connection.DbConnectionName)
		            .UsingFactoryMethod(kernel => new SqlConnection(_connectionString))
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
		        Component.For<IDapperProvider<TConnection>>()
		            .Named(_connection.FactoryName)
		            .AsFactory(cfg => cfg.SelectedWith(_connection.SelectorName)));
		}

		private Func<bool> SaveChanges
		{
			get
			{
				if (!HostingEnvironment.IsHosted)
					return () => !_unhandledException;

				return () => HttpContext.Server.GetLastError() == null;
			}
		}

		protected TConnection Connection
		{
			get { return _connection; }
		}

		private HttpContextBase HttpContext
		{
			get 
			{
				var ctx = System.Web.HttpContext.Current;

				if (ctx != null)
					return new HttpContextWrapper(ctx);

				return null;
			}
		}
	}
}