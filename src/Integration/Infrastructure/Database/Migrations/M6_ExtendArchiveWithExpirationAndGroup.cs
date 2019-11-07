using System;
using FluentMigrator;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    [Migration(6)]
    public class M6_ExtendArchiveWithExpirationAndGroup : IntegrationMigration
    {
        public override void Up()
        {
            var configuration = Resolve<IIntegrationDatabaseConfiguration>();

            Alter.Table(configuration.TableName(IntegrationDbTable.Archive))
                .AddColumn("Expires").AsDateTimeOffset().Nullable()
                .AddColumn("GroupName").AsString().Nullable();
        }

        public override void Down()
        {
            throw new NotSupportedException("Migrating down is not supported.");
        }
    }
}