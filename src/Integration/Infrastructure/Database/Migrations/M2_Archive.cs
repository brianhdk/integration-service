using System;
using FluentMigrator;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    [Migration(2)]
    public class M2_Archive : IntegrationMigration
    {
        public override void Up()
        {
            var configuration = Resolve<IIntegrationDatabaseConfiguration>();

            Create.Table(configuration.TableName(IntegrationDbTable.Archive))
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString(255)
                .WithColumn("BinaryData").AsBinary(int.MaxValue)
                .WithColumn("ByteSize").AsInt32()
                .WithColumn("Created").AsDateTimeOffset();
        }

        public override void Down()
        {
            throw new NotSupportedException("Migrating down is not supported.");
        }
    }
}