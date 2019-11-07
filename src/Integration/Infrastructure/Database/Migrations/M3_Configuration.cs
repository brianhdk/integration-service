using System;
using FluentMigrator;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    [Migration(3)]
    public class M3_Configuration : IntegrationMigration
    {
        public override void Up()
        {
            var configuration = Resolve<IIntegrationDatabaseConfiguration>();

            Create.Table(configuration.TableName(IntegrationDbTable.Configuration))
                .WithColumn("Id").AsString(255).PrimaryKey()
                .WithColumn("Name").AsString(50)
                .WithColumn("Description").AsString(255).Nullable()
                .WithColumn("JsonData").AsString(int.MaxValue)
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