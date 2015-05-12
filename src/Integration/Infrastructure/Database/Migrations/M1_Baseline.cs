using System;
using FluentMigrator;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    [Migration(1)]
	public class M1_Baseline : Migration
	{
		public override void Up()
		{
            Create.Table("ErrorLog")
				.WithColumn("Id").AsInt32().PrimaryKey().Identity()
				.WithColumn("MachineName").AsString(255)
                .WithColumn("IdentityName").AsString(255).Nullable()
                .WithColumn("CommandLine").AsString(Int32.MaxValue).Nullable()
                .WithColumn("Message").AsString(Int32.MaxValue).Nullable()
                .WithColumn("FormattedMessage").AsCustom("NTEXT").Nullable()
                .WithColumn("Severity").AsString(50)
                .WithColumn("Target").AsString(50)
				.WithColumn("TimeStamp").AsDateTimeOffset();

            Create.Table("TaskLog")
				.WithColumn("Id").AsInt32().PrimaryKey().Identity()
				.WithColumn("Type").AsAnsiString(1)
				.WithColumn("TaskName").AsString(255)
				.WithColumn("StepName").AsString(255).Nullable()
				.WithColumn("Message").AsString(Int32.MaxValue).Nullable()
				.WithColumn("ExecutionTimeSeconds").AsDouble()
				.WithColumn("TimeStamp").AsDateTimeOffset()
				.WithColumn("TaskLog_Id").AsInt32().Nullable()
				.WithColumn("StepLog_Id").AsInt32().Nullable()
				.WithColumn("ErrorLog_Id").AsInt32().Nullable();
		}

		public override void Down()
		{
		    throw new NotSupportedException("Migrating down is not supported.");
		}
	}
}