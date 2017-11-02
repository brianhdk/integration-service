using System;
using System.Data;
using Vertica.Integration.Infrastructure.Archiving;
using Vertica.Integration.Infrastructure.Configuration;
using Vertica.Integration.Infrastructure.Database;
using Vertica.Integration.Infrastructure.Database.Extensions;
using Vertica.Integration.Model;
using Vertica.Utilities;

namespace Vertica.Integration.Domain.Core
{
	public class CleanUpIntegrationDbStep : Step<MaintenanceWorkItem>
	{
	    private readonly Lazy<IDbFactory> _db;
	    private readonly IIntegrationDatabaseConfiguration _dbConfiguration;
	    private readonly IConfigurationService _configuration;
	    private readonly IArchiveService _archiver;

		public CleanUpIntegrationDbStep(Lazy<IDbFactory> db, IIntegrationDatabaseConfiguration dbConfiguration, IConfigurationService configuration, IArchiveService archiver)
		{
		    _db = db;
		    _dbConfiguration = dbConfiguration;
		    _configuration = configuration;
		    _archiver = archiver;
		}

        public override Execution ContinueWith(ITaskExecutionContext<MaintenanceWorkItem> context)
        {
            if (_dbConfiguration.Disabled)
                return Execution.StepOver;

            return Execution.Execute;
        }

        public override void Execute(ITaskExecutionContext<MaintenanceWorkItem> context)
        {
			DateTimeOffset tasksLowerBound = Time.UtcNow.Subtract(context.WorkItem.Configuration.CleanUpTaskLogEntriesOlderThan),
				errorsLowerBound = Time.UtcNow.Subtract(context.WorkItem.Configuration.CleanUpErrorLogEntriesOlderThan);

			using (IDbSession session = _db.Value.OpenSession())
			using (IDbTransaction transaction = session.BeginTransaction())
			{
			    Tuple<int, string> taskLog = DeleteEntries(session, IntegrationDbTable.TaskLog, tasksLowerBound);
			    Tuple<int, string> errorLog = DeleteEntries(session, IntegrationDbTable.ErrorLog, errorsLowerBound);

				transaction.Commit();

			    if (taskLog.Item1 > 0 || errorLog.Item1 > 0)
			    {
			        ArchiveCreated archive = _archiver.Archive("IntegrationDb-Maintenance", a =>
			        {
			            a.Options.GroupedBy("Backup").ExpiresAfterMonths(12);

			            a.IncludeContent($"TaskLog_{tasksLowerBound:yyyyMMdd}.csv", taskLog.Item2);
			            a.IncludeContent($"ErrorLog_{errorsLowerBound:yyyyMMdd}.csv", errorLog.Item2);
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

	    private Tuple<int, string> DeleteEntries(IDbSession session, IntegrationDbTable table, DateTimeOffset lowerBound)
	    {
	        return session.Wrap(s =>
	        {
	            string query = $" FROM [{_dbConfiguration.TableName(table)}] WHERE [TimeStamp] <= @lowerbound";

	            string csv = s.QueryToCsv(string.Concat("SELECT *", query), new { lowerBound });
                int count = s.Execute(string.Concat("DELETE", query), new { lowerBound }, commandTimeout: 10800);

	            return Tuple.Create(count, csv);
	        });
	    }

        public override string Description
		{
			get
			{
                MaintenanceConfiguration configuration = _configuration.Get<MaintenanceConfiguration>();

			    return
				    $"Deletes TaskLog entries older than {configuration.CleanUpTaskLogEntriesOlderThan.TotalDays} days and ErrorLog entries older than {configuration.CleanUpErrorLogEntriesOlderThan.TotalDays} days from IntegrationDb.";
			}
		}
	}
}