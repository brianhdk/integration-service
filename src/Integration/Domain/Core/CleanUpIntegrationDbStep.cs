using System;
using System.Data;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Model;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Extensions.TimeExt;

namespace Vertica.Integration.Domain.Core
{
	public class CleanUpIntegrationDbStep : Step<MaintenanceWorkItem>
	{
	    private readonly IDbFactory _db;
	    private readonly IConfigurationService _configuration;

		public CleanUpIntegrationDbStep(IDbFactory db, IConfigurationService configuration)
		{
		    _db = db;
		    _configuration = configuration;
		}

        public override void Execute(MaintenanceWorkItem workItem, ITaskExecutionContext context)
        {
			DateTimeOffset tasksLowerBound = Time.UtcNow.BeginningOfDay().Subtract(workItem.Configuration.CleanUpTaskLogEntriesOlderThan),
				errorsLowerBound = Time.UtcNow.BeginningOfDay().Subtract(workItem.Configuration.CleanUpErrorLogEntriesOlderThan);

			using (IDbSession session = _db.OpenSession())
			using (IDbTransaction transaction = session.BeginTransaction())
			{
				int taskCount = DeleteTaskEntries(session, tasksLowerBound);
				int errorCount = DeleteErrorEntries(session, errorsLowerBound);

				transaction.Commit();

				if (taskCount > 0)
					context.Log.Message("Deleted {0} task entries older than '{1}'.", taskCount, tasksLowerBound);

				if (errorCount > 0)
                    context.Log.Message("Deleted {0} error entries older than '{1}'.", errorCount, errorsLowerBound);
			}
		}

		private int DeleteTaskEntries(IDbSession session, DateTimeOffset lowerBound)
		{
		    return session.ExecuteScalar<int>("DELETE FROM [TaskLog] WHERE [TimeStamp] <= @lowerbound", new { lowerBound });
		}

		private int DeleteErrorEntries(IDbSession session, DateTimeOffset lowerBound)
		{
            return session.ExecuteScalar<int>("DELETE FROM [ErrorLog] WHERE [TimeStamp] <= @lowerBound", new { lowerBound });
		}

        public override string Description
		{
			get
			{
                MaintenanceConfiguration configuration = _configuration.Get<MaintenanceConfiguration>();

			    return
			        String.Format(
			            "Deletes TaskLog entries older than {0} days and ErrorLog entries older than {1} days from IntegrationDb.",
			            configuration.CleanUpTaskLogEntriesOlderThan.TotalDays,
			            configuration.CleanUpErrorLogEntriesOlderThan.TotalDays);
			}
		}
	}
}