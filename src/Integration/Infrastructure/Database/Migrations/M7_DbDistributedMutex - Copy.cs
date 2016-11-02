using System;
using FluentMigrator;
using Vertica.Integration.Infrastructure.Database.Migrations.Features;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    [Migration(8)]
    [DbDistributedMutexFeature]
    public class M8_DbDistributedMutexAddDescription : IntegrationMigration
    {
        public override void Up()
        {
            var configuration = Resolve<IIntegrationDatabaseConfiguration>();

            Alter.Table(configuration.TableName(IntegrationDbTable.DistributedMutex))
                .AddColumn("Description").AsString(255).Nullable();
        }

        public override void Down()
        {
            throw new NotSupportedException("Migrating down is not supported.");
        }
    }
}