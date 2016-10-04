using System;
using FluentMigrator;
using Vertica.Integration.Infrastructure.Database.Migrations.Features;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    [Migration(5)]
    [DbLoggerFeature]
    public class M5_TaskLogNullableExecutionTimeSeconds : Migration
    {
        public override void Up()
        {
            Alter.Table("TaskLog")
                .AlterColumn("ExecutionTimeSeconds").AsDouble().Nullable();

            Execute.Sql("UPDATE TaskLog SET ExecutionTimeSeconds = NULL WHERE ([Type] = 'M');");
        }

        public override void Down()
        {
            throw new NotSupportedException("Migrating down is not supported.");
        }
    }
}