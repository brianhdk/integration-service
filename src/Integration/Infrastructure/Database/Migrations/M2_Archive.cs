using System;
using FluentMigrator;

namespace Vertica.Integration.Infrastructure.Database.Migrations
{
    [Migration(2)]
    public class M2_Archive : Migration
    {
        public override void Up()
        {
            Create.Table("Archive")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("Name").AsString(50)
                .WithColumn("BinaryData").AsBinary(Int32.MaxValue)
                .WithColumn("ByteSize").AsInt32()
                .WithColumn("Created").AsDateTimeOffset();
        }

        public override void Down()
        {
            throw new NotSupportedException("Migrating down is not supported.");
        }
    }
}