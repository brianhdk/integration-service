using System;
using Castle.Windsor;
using NHibernate;
using NHibernate.Context;
using NHibernate.Engine;
using Vertica.Integration.Infrastructure.Factories;

namespace Vertica.Integration.Infrastructure.Database.NHibernate.Castle.Windsor
{
    public class CurrentSessionContext : ICurrentSessionContext
    {
        private readonly ISessionFactoryImplementor _sessionFactory;
	    private readonly IWindsorContainer _container; 

        public CurrentSessionContext(ISessionFactoryImplementor sessionFactory)
        {
            _sessionFactory = sessionFactory;
	        _container = ObjectFactory.Instance;
        }

        public ISession CurrentSession()
        {
	        var sessions = _container.ResolveAll<ICurrentSession>();

	        foreach (ICurrentSession session in sessions)
	        {
		        if (session.SessionFactory == _sessionFactory)
			        return session;
	        }

	        throw new InvalidOperationException("No provider found for session factory.");
        }
    }
}