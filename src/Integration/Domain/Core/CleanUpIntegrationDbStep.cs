using System;
using System.Data;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Database.Extensions;
using Vertica.Integration.Model;
using Vertica.Utilities_v4;
using Vertica.Utilities_v4.Extensions.TimeExt;

namespace Vertica.Integration.Domain.Core
{
	public class CleanUpIntegrationDbStep : Step<MaintenanceWorkItem>
	{
	    private readonly IDbFactory _db;
	    private readonly IConfigurationService _configuration;
	    private readonly IArchiveService _archiver;

		public CleanUpIntegrationDbStep(IDbFactory db, IConfigurationService configuration, IArchiveService archiver)
		{
		    _db = db;
		    _configuration = configuration;
		    _archiver = archiver;
		}

        public override void Execute(MaintenanceWorkItem workItem, ITaskExecutionContext context)
        {
			DateTimeOffset tasksLowerBound = Time.UtcNow.Subtract(workItem.Configuration.CleanUpTaskLogEntriesOlderThan),
				errorsLowerBound = Time.UtcNow.Subtract(workItem.Configuration.CleanUpErrorLogEntriesOlderThan);

			using (IDbSession session = _db.OpenSession())
			using (IDbTransaction transaction = session.BeginTransaction())
			{
			    Tuple<int, string> taskLog = DeleteEntries(session, "TaskLog", tasksLowerBound);
			    Tuple<int, string> errorLog = DeleteEntries(session, "ErrorLog", errorsLowerBound);

				transaction.Commit();

			    if (taskLog.Item1 > 0 || errorLog.Item1 > 0)
			    {
			        ArchiveCreated archive = _archiver.Archive("IntegrationDb-Maintenance", a =>
			        {
			            a.Options.GroupedBy("Backup").ExpiresAfterMonths(12);

			            a.IncludeContent(String.Format("TaskLog_{0:yyyyMMdd}.csv", tasksLowerBound), taskLog.Item2);
			            a.IncludeContent(String.Format("ErrorLog_{0:yyyyMMdd}.csv", errorsLowerBound), errorLog.Item2);
			        });

			        context.Log.Message(@"Deleted {0} task entries older than '{1}'. 
Deleted {2} error entries older than '{3}'
Archive: {4}",
			            taskLog.Item1,
			            tasksLowerBound,
			            errorLog.Item1,
			            errorsLowerBound,
			            archive);
			    }
			}
		}

	    private Tuple<int, string> DeleteEntries(IDbSession session, string tableName, DateTimeOffset lowerBound)
	    {
	        return session.Wrap(s =>
	        {
	            string query = String.Format(" FROM [{0}] WHERE [TimeStamp] <= @lowerbound", tableName);

	            string csv = s.QueryToCsv(String.Concat("SELECT *", query), new { lowerBound });
                int count = s.Execute(String.Concat("DELETE", query), new { lowerBound });

	            return Tuple.Create(count, csv);
	        });
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