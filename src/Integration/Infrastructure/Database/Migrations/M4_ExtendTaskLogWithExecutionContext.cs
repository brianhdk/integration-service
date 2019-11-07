using System;
using FluentMigrator;
using Vertica.Integration.Infrastructure.Database.Migrations.Features;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    [Migration(4)]
    [DbLoggerFeature]
    public class M4_ExtendTaskLogWithExecutionContext : IntegrationMigration
    {
        public override void Up()
        {
            var configuration = Resolve<IIntegrationDatabaseConfiguration>();

            Alter.Table(configuration.TableName(IntegrationDbTable.TaskLog))
				.AddColumn("MachineName").AsString(255).Nullable()
                .AddColumn("IdentityName").AsString(255).Nullable()
                .AddColumn("CommandLine").AsString(int.MaxValue).Nullable();
        }

        public override void Down()
        {
            throw new NotSupportedException("Migrating down is not supported.");
        }
    }
}