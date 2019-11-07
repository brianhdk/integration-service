using System;
using System.IO.Compression;
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
			MaintenanceConfiguration configuration = _configuration.Get<MaintenanceConfiguration>();

			DateTimeOffset tasksLowerBound = Time.UtcNow.Subtract(context.WorkItem.Configuration.CleanUpTaskLogEntriesOlderThan),
				errorsLowerBound = Time.UtcNow.Subtract(context.WorkItem.Configuration.CleanUpErrorLogEntriesOlderThan);

			using (IDbSession session = _db.Value.OpenSession())
			{
			    Tuple<int, string> taskLog = DeleteEntries(configuration, session, IntegrationDbTable.TaskLog, tasksLowerBound);
			    Tuple<int, string> errorLog = DeleteEntries(configuration, session, IntegrationDbTable.ErrorLog, errorsLowerBound);

			    if (taskLog.Item1 > 0 || errorLog.Item1 > 0)
			    {
			        ArchiveCreated archive = null;

			        if (configuration.ArchiveDeletedLogEntries)
			        {
			            archive = _archiver.Archive("IntegrationDb-Maintenance", a =>
			            {
			                a.Options
			                    .GroupedBy("Backup")
			                    .ExpiresAfterMonths(12)
			                    .Compression(CompressionLevel.Optimal);

			                a.IncludeContent($"TaskLog_{tasksLowerBound:yyyyMMdd}.csv", taskLog.Item2);
			                a.IncludeContent($"ErrorLog_{errorsLowerBound:yyyyMMdd}.csv", errorLog.Item2);
			            });
			        }

			        context.Log.Message(@"Deleted {0} task entries older than '{1}'. 
Deleted {2} error entries older than '{3}'
Archive: {4}",
			            taskLog.Item1,
			            tasksLowerBound,
			            errorLog.Item1,
			            errorsLowerBound,
			            archive ?? "<Not archived>");
			    }
			}
		}

	    private Tuple<int, string> DeleteEntries(MaintenanceConfiguration configuration, IDbSession session, IntegrationDbTable table, DateTimeOffset lowerBound)
	    {
	        return session.Wrap(s =>
	        {
	            string csv = null;
	            string tableName = _dbConfiguration.TableName(table);
	            int batchSize = configuration.DeleteLogEntriesBatchSize;

	            if (configuration.ArchiveDeletedLogEntries)
	            {
	                string query = $"SELECT * FROM [{tableName}] WHERE [TimeStamp] <= @lowerbound";
	                csv = s.QueryToCsv(query, new { lowerBound });
	            }

	            int count = s.Execute(GetDeleteSql(tableName), new { lowerBound, batchSize }, configuration.CleanUpCommandTimeout.GetValueOrDefault(10800));

	            return Tuple.Create(count, csv);
	        });
	    }

	    private static string GetDeleteSql(string tableName)
	    {
	        return $@"
DECLARE @DELETEDCOUNT INT
SET @DELETEDCOUNT = 0

DECLARE @BATCHCOUNT INT
SET @BATCHCOUNT = @batchSize

WHILE @BATCHCOUNT > 0
BEGIN
    DELETE TOP(@BATCHCOUNT) FROM [{tableName}] WHERE TimeStamp <= @lowerbound
    SET @BATCHCOUNT = @@ROWCOUNT
    SET @DELETEDCOUNT = @DELETEDCOUNT + @BATCHCOUNT

    IF @BATCHCOUNT > 0
    BEGIN
        WAITFOR DELAY '00:00:02' -- wait 2 secs to allow other processes access to the table
    END
END

SELECT @DELETEDCOUNT";
	    }

        public override string Description
		{
			get
			{
                MaintenanceConfiguration configuration = _configuration.Get<MaintenanceConfiguration>();

			    return $"Deletes TaskLog entries older than {configuration.CleanUpTaskLogEntriesOlderThan.TotalDays} days and ErrorLog entries older than {configuration.CleanUpErrorLogEntriesOlderThan.TotalDays} days from IntegrationDb.";
			}
		}
	}
}