using System;
using System.Data;
using Vertica.Integration.Infrastructure.Database.Dapper;
using Vertica.Integration.Model;
using Vertica.Utilities_v4.Extensions.StringExt;
using Vertica.Utilities_v4.Extensions.TimeExt;

namespace Vertica.Integration.Domain.Core
{
	public class CleanUpIntegrationDbStep : Step<MaintenanceWorkItem>
	{
	    private readonly IDapperProvider _dapper;
	    private readonly TimeSpan _tasksOlderThan;
		private readonly TimeSpan _errorsOlderThan;

		public CleanUpIntegrationDbStep(IDapperProvider dapper, TimeSpan tasksOlderThan, TimeSpan errorsOlderThan)
		{
		    _dapper = dapper;
		    _tasksOlderThan = tasksOlderThan;
			_errorsOlderThan = errorsOlderThan;
		}

        public override void Execute(MaintenanceWorkItem workItem, Log log)
		{
			DateTimeOffset tasksLowerBound = DateTimeOffset.UtcNow.BeginningOfDay().Subtract(_tasksOlderThan),
				errorsLowerBound = DateTimeOffset.UtcNow.BeginningOfDay().Subtract(_errorsOlderThan);

			using (IDapperSession session = _dapper.OpenSession())
			using (IDbTransaction transaction = session.BeginTransaction())
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

		private int DeleteTaskEntries(IDapperSession session, DateTimeOffset lowerBound)
		{
		    return session.ExecuteScalar<int>("DELETE FROM [TaskLog] WHERE [TimeStamp] <= @lowerbound", new { lowerBound });
		}

		private int DeleteErrorEntries(IDapperSession session, DateTimeOffset lowerBound)
		{
            return session.ExecuteScalar<int>("DELETE FROM [ErrorLog] WHERE [TimeStamp] <= @lowerBound", new { lowerBound });
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