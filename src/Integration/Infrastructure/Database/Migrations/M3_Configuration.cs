using System;
using FluentMigrator;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    [Migration(3)]
    public class M3_Configuration : Migration
    {
        public override void Up()
        {
            Create.Table("Configuration")
                .WithColumn("ClrType").AsString(255).PrimaryKey()
                .WithColumn("JsonData").AsString(Int32.MaxValue)
                .WithColumn("Created").AsDateTimeOffset()
                .WithColumn("Updated").AsDateTimeOffset()
                .WithColumn("UpdatedBy").AsString(50);
        }

        public override void Down()
        {
            throw new NotSupportedException("Migrating down is not supported.");
        }
    }
}