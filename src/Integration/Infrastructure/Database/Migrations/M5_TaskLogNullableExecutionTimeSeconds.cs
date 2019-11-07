using System;
using FluentMigrator;
using Vertica.Integration.Infrastructure.Database.Migrations.Features;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    [Migration(5)]
    [DbLoggerFeature]
    public class M5_TaskLogNullableExecutionTimeSeconds : IntegrationMigration
    {
        public override void Up()
        {
            var configuration = Resolve<IIntegrationDatabaseConfiguration>();

            Alter.Table(configuration.TableName(IntegrationDbTable.TaskLog))
                .AlterColumn("ExecutionTimeSeconds").AsDouble().Nullable();

            Execute.Sql($"UPDATE [{configuration.TableName(IntegrationDbTable.TaskLog)}] SET ExecutionTimeSeconds = NULL WHERE ([Type] = 'M');");
        }

        public override void Down()
        {
            throw new NotSupportedException("Migrating down is not supported.");
        }
    }
}