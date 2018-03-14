using System;
using FluentMigrator;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    [Migration(9)]
    public class M9_CreateLogTableIndicies : IntegrationMigration
    {
        public override void Up()
        {
            var configuration = Resolve<IIntegrationDatabaseConfiguration>();

            string taskLogTableName = configuration.TableName(IntegrationDbTable.TaskLog);
            string taskLogIndexName = $"IX_{taskLogTableName}_ErrorLog_Id";

            string errorLogTableName = configuration.TableName(IntegrationDbTable.ErrorLog);
            string errorLogIndexName = $"IX_{errorLogTableName}_TimeStamp";

            Execute.Sql($@"
IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('{taskLogTableName}') AND NAME = '{taskLogIndexName}')
    DROP INDEX [{taskLogIndexName}] ON [{taskLogTableName}];
GO

CREATE NONCLUSTERED INDEX [{taskLogIndexName}] ON [{taskLogTableName}]
(
	[ErrorLog_Id] ASC
)
INCLUDE (
    [Id],
	[TaskName],
	[StepName]
) 
WITH (
    PAD_INDEX = OFF, 
    STATISTICS_NORECOMPUTE = OFF, 
    SORT_IN_TEMPDB = OFF, 
    DROP_EXISTING = OFF, 
    ONLINE = OFF, 
    ALLOW_ROW_LOCKS = ON, 
    ALLOW_PAGE_LOCKS = ON
)

IF EXISTS(SELECT * FROM sys.indexes WHERE object_id = object_id('{errorLogTableName}') AND NAME = '{errorLogIndexName}')
    DROP INDEX [{errorLogIndexName}] ON [{errorLogTableName}];
GO

CREATE NONCLUSTERED INDEX [{errorLogIndexName}] ON [{errorLogTableName}]
(
	[TimeStamp] ASC
)
INCLUDE ( 	
    [Id],
	[Message],
	[Severity],
	[Target]
)
WITH (
    PAD_INDEX = OFF, 
    STATISTICS_NORECOMPUTE = OFF, 
    SORT_IN_TEMPDB = OFF, 
    DROP_EXISTING = OFF, 
    ONLINE = OFF, 
    ALLOW_ROW_LOCKS = ON, 
    ALLOW_PAGE_LOCKS = ON
)
");
        }

        public override void Down()
        {
            throw new NotSupportedException("Migrating down is not supported.");
        }
    }
}