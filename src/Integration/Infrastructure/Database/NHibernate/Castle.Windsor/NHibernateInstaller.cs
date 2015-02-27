using System;
using System.Web;
using System.Web.Hosting;
using Castle.Facilities.TypedFactory;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using Environment = NHibernate.Cfg.Environment;

namespace Vertica.Integration.Infrastructure.Database.NHibernate.Castle.Windsor
{
    internal class NHibernateInstaller : NHibernateInstaller<DefaultConnection>
	{
	    private static readonly Lazy<Action> EnsureSingleton = new Lazy<Action>(() => () => { });
 
		public NHibernateInstaller(DefaultConnection connection)
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
				Component.For<ISessionFactoryProvider>()
				         .AsFactory(cfg => cfg.SelectedWith(Connection.SelectorName)));

            // todo: kan vi registrere ISessionFactoryProvider<konkret-connection> - og når man beder om den så hente den ud fra default connection component navnet?
            //container.Register(
            //    Component.For<ISessionFactoryProvider<TConnection>>()
            //             .Named(Connection.FactoryName)
            //             .AsFactory(cfg => cfg.SelectedWith(_connection.SelectorName)));
		}
	}

	public class NHibernateInstaller<TConnection> : IWindsorInstaller
		where TConnection : Connection
	{
		private readonly TConnection _connection;
		private bool _unhandledException;

		public NHibernateInstaller(TConnection connection)
		{
			if (connection == null) throw new ArgumentNullException("connection");

			_connection = connection;

			if (!HostingEnvironment.IsHosted)
				AppDomain.CurrentDomain.UnhandledException += (obj, ex) => _unhandledException = true;
		}

		public virtual void Install(IWindsorContainer container, IConfigurationStore store)
		{
			container.Register(
				Component.For<ISessionFactory>()
					.Named(_connection.SessionFactoryName)
					.UsingFactoryMethod(kernel =>
					{
						ISessionFactory factory =
							Fluently.Configure()
								.ExposeConfiguration(config =>
								{
								    config.SetProperty(Environment.CurrentSessionContextClass, typeof(CurrentSessionContext).AssemblyQualifiedName);

								    _connection.SetupConfiguration(config);
								})
								.Database(MsSqlConfiguration.MsSql2008.ConnectionString(c => c.FromConnectionStringWithKey(_connection.ConnectionStringName)))
								.Mappings(_connection.SetupMappings)
								.BuildSessionFactory();

						_connection.OnSessionFactoryCreated(factory);

						return factory;
					}));

			var currentSessionComponent =
				Component.For<ICurrentSession>()
						 .Named(_connection.CurrentSessionName)
						 .UsingFactoryMethod<ICurrentSession>(kernel => 
							 new CurrentSession(
								 kernel.Resolve<ISessionFactory>(_connection.SessionFactoryName), 
								 SaveChanges));

			container.Register(
				HostingEnvironment.IsHosted
					? currentSessionComponent.LifestylePerWebRequest()
					: currentSessionComponent.LifestylePerThread());

			container.Register(
				Component.For<NHibernateSelector<TConnection>>()
						 .Named(_connection.SelectorName)
				         .UsingFactoryMethod(() => new NHibernateSelector<TConnection>(_connection)));

			container.Register(
				Component.For<ISessionFactoryProvider<TConnection>>()
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