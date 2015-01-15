using System;
using NHibernate;
using Vertica.Integration.Infrastructure.Database.NHibernate;

namespace Vertica.Integration.Infrastructure.Configuration
{
	public class ParametersProvider : IParametersProvider
	{
		private readonly Lazy<ISessionFactoryProvider> _sessionFactoryProvider;

		public ParametersProvider(Lazy<ISessionFactoryProvider> sessionFactoryProvider)
		{
			_sessionFactoryProvider = sessionFactoryProvider;
		}

		public Parameters Get()
		{
			ISessionFactory sessionFactory = _sessionFactoryProvider.Value.SessionFactory;

			using (IStatelessSession session = sessionFactory.OpenStatelessSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				var parameters =
					session.QueryOver<Parameters>().SingleOrDefault();

				if (parameters == null)
				{
					parameters = new Parameters();
					session.Insert(parameters);
					transaction.Commit();
				}

				return parameters;				
			}
		}

		public void Save(Parameters parameters)
		{
			if (parameters == null) throw new ArgumentNullException("parameters");

			ISessionFactory sessionFactory = _sessionFactoryProvider.Value.SessionFactory;

			using (IStatelessSession session = sessionFactory.OpenStatelessSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				session.Update(parameters);
				transaction.Commit();
			}
		}
	}
}