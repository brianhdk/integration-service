using System;
using FluentMigrator;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    [Migration(VersionNumber)]
    public class M4_ExtendTaskLogWithExecutionContext : Migration
    {
        public const int VersionNumber = 4;

        public override void Up()
        {
            Alter.Table("TaskLog")
				.AddColumn("MachineName").AsString(255).Nullable()
                .AddColumn("IdentityName").AsString(255).Nullable()
                .AddColumn("CommandLine").AsString(Int32.MaxValue).Nullable();
        }

        public override void Down()
        {
            throw new NotSupportedException("Migrating down is not supported.");
        }
    }
}