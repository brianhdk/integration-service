using System;
using FluentMigrator;
using Vertica.Integration.Infrastructure.Database.Migrations.Features;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    [Migration(1)]
    [DbLoggerFeature]
	public class M1_Baseline : IntegrationMigration
	{
		public override void Up()
		{
		    var configuration = Resolve<IIntegrationDatabaseConfiguration>();

            Create.Table(configuration.TableName(IntegrationDbTable.ErrorLog))
				.WithColumn("Id").AsInt32().PrimaryKey().Identity()
				.WithColumn("MachineName").AsString(255)
                .WithColumn("IdentityName").AsString(255).Nullable()
                .WithColumn("CommandLine").AsString(int.MaxValue).Nullable()
                .WithColumn("Message").AsString(int.MaxValue).Nullable()
                .WithColumn("FormattedMessage").AsCustom("NTEXT").Nullable()
                .WithColumn("Severity").AsString(50)
                .WithColumn("Target").AsString(50)
				.WithColumn("TimeStamp").AsDateTimeOffset();

            Create.Table(configuration.TableName(IntegrationDbTable.TaskLog))
				.WithColumn("Id").AsInt32().PrimaryKey().Identity()
				.WithColumn("Type").AsAnsiString(1)
				.WithColumn("TaskName").AsString(255)
				.WithColumn("StepName").AsString(255).Nullable()
				.WithColumn("Message").AsString(int.MaxValue).Nullable()
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