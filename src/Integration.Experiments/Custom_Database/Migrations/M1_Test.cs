using System;
using FluentMigrator;

namespace Vertica.Integration.Experiments.Custom_Database.Migrations
{
    [Migration(1)]
    public class M1_Test : Migration
    {
        public override void Up()
        {
            Create.Table("Test")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity();
        }

        public override void Down()
        {
            throw new NotSupportedException(@"Not supported.");
        }
    }
}