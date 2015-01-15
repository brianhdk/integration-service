using System;
using NHibernate;
using Vertica.Integration.Infrastructure.Database.NHibernate;
using Vertica.Integration.Model;
using Vertica.Utilities_v4.Extensions.StringExt;
using Vertica.Utilities_v4.Extensions.TimeExt;

namespace Vertica.Integration.Domain.Core
{
	public class CleanUpIntegrationDbStep : Step<MaintenanceWorkItem>
	{
	    private readonly Lazy<ISessionFactoryProvider> _sessionFactory;
	    private readonly TimeSpan _tasksOlderThan;
		private readonly TimeSpan _errorsOlderThan;

		public CleanUpIntegrationDbStep(Lazy<ISessionFactoryProvider> sessionFactory, TimeSpan tasksOlderThan, TimeSpan errorsOlderThan)
		{
		    _sessionFactory = sessionFactory;
		    _tasksOlderThan = tasksOlderThan;
			_errorsOlderThan = errorsOlderThan;
		}

        public override void Execute(MaintenanceWorkItem workItem, Log log)
		{
			DateTimeOffset tasksLowerBound = DateTimeOffset.UtcNow.BeginningOfDay().Subtract(_tasksOlderThan),
				errorsLowerBound = DateTimeOffset.UtcNow.BeginningOfDay().Subtract(_errorsOlderThan);

			using (IStatelessSession session = _sessionFactory.Value.SessionFactory.OpenStatelessSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				int taskCount = DeleteTaskEntries(session, tasksLowerBound);
				int errorCount = DeleteErrorEntries(session, errorsLowerBound);

				transaction.Commit();

				if (taskCount > 0)
					log.Message("Deleted {0} task entries older than '{1}'.", taskCount, tasksLowerBound);

				if (errorCount > 0)
					log.Message("Deleted {0} error entries older than '{1}'.", errorCount, errorsLowerBound);
			}
		}

		private int DeleteTaskEntries(IStatelessSession session, DateTimeOffset lowerBound)
		{
		    return
		        session.CreateSQLQuery("DELETE FROM [TaskLog] WHERE [TimeStamp] <= :timestamp")
		            .SetParameter("timestamp", lowerBound)
		            .ExecuteUpdate();
		}

		private int DeleteErrorEntries(IStatelessSession session, DateTimeOffset lowerBound)
		{
            return
                session.CreateSQLQuery("DELETE FROM [ErrorLog] WHERE [TimeStamp] <= :timestamp")
                    .SetParameter("timestamp", lowerBound)
                    .ExecuteUpdate();
		}

        public override string Description
		{
			get
			{
				return "Deletes integration task entries older than '{0}' days and error entries older than '{1}'"
					.FormatWith(_tasksOlderThan.TotalDays, _errorsOlderThan.TotalDays);
			}
		}
	}
}